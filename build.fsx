// include Fake libs
#r "tools/FAKE/FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open Fake.Git
open Fake.TeamCityHelper
open Fake.Testing.NUnit3
open Fake.XDTHelper

open System
open System.IO
open System.Text.RegularExpressions
open System.Xml

// Directories
let buildAppDir  = "./build/"
let buildTestDir   = "./test/"
let deployDir = "./deploy/"

// Filesets
let appReferences  =
    !! "src/**/*.csproj"
      -- "src/**/*.Tests.csproj"
      -- "src/**/*.Tests.*.csproj"
      -- "src/**/*.Tests.*.*.csproj"
      -- "src/**/**/**/**/*.Tests.*.*.csproj"
 

let unitTestReferences =
    !! "src/**/*.Tests.Unit.csproj"
      ++ "src/**/*.Tests.Unit.*.csproj"

let otherTestReferences =
    !! "src/**/*.Tests.csproj"
      ++ "src/**/*.Tests.*.csproj"
      ++ "src/**/*.Tests.*.*.csproj" 
      ++ "src/**/*.Tests.*.csproj"
      ++ "src/**/**/*.Tests.*.*.csproj"
      ++ "src/**/**/**/*.Tests.*.*.csproj"
      ++ "src/**/**/**/**/*.Tests.*.*.csproj"
      -- "src/**/*.Tests.Unit.csproj"
      -- "src/**/*.Tests.Unit.*.csproj"

// Release
let releaseNotesData =
    File.ReadAllLines "RELEASE_NOTES.md"
    |> parseAllReleaseNotes

let release = List.head releaseNotesData

let buildNumber =
    match isLocalBuild with
    | true -> "0"
    | _ -> getBuildParamOrDefault "BuildCounter" buildVersion

let finalBuildVersion = sprintf "%s.%s" release.AssemblyVersion buildNumber

if not isLocalBuild then
    SetBuildNumber finalBuildVersion

// Build defaults
let isCoverage = hasBuildParam "Coverage"
let buildConfiguration = if isCoverage then "Debug" else "Release"
let transformConfiguration = getBuildParamOrDefault "config" buildConfiguration

// special case for this project
MSBuildDefaults <- { MSBuildDefaults with Properties = [ ("RestorePackages", "true") ] }

let MSBuildWithCustomTransform outputDir projects =
    traceStartTask "MSBuildWithCustomTransform" (sprintf "%A" projects)

    projects
    |> Seq.iter (fun file ->
        if ((file:string).IndexOf "NoTest") < 0 then
            let directory = (DirectoryName file)
            let projectKey = (fileNameWithoutExt file)
            let outputPath = outputDir @@ projectKey

            MSBuild outputPath "Build" [ "Configuration", buildConfiguration ] [ file ]
                |> Log (sprintf "%s-Output: " projectKey)

            !! (outputPath @@ (sprintf "*%s.*.config" projectKey))
            |> Seq.iter (fun configFile ->
                let sourceFile = directory @@ (if configFile.EndsWith("web.config", StringComparison.OrdinalIgnoreCase) then "web.config" else "app.config")
                let transformConfig = (DirectoryName sourceFile) @@ sprintf "%s.%s.config" (fileNameWithoutExt sourceFile) transformConfiguration

                if fileExists transformConfig then
                    trace (sprintf "%s <- %s => %s" sourceFile transformConfig configFile)
                    TransformFile sourceFile transformConfig configFile
            )
    )

    traceEndTask "MSBuildWithCustomTransform" (sprintf "%A" projects)

// Release
let gitApi = "https://github.exacttarget.com/api/v3/"
let gitOwner = "ad-studio"
let gitHome = "git@github.exacttarget.com:" + gitOwner
let gitName = "magiql"

// --------------------------------------------------------------------------------------
// Dependencies
// --------------------------------------------------------------------------------------

#load "src/paket-files/build/fsharp/FAKE/modules/Octokit/Octokit.fsx"
open Octokit

// --------------------------------------------------------------------------------------
// Extend Octokit
// --------------------------------------------------------------------------------------

let setBuildNumberFromDraft (draft : Async<Draft>) = async {
    let! draft' = draft

    if not isLocalBuild then
        let (|Regex|_|) pattern input =
                let m = Regex.Match(input, pattern)
                if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
                else None

        match draft'.DraftRelease.Body with
            | Regex @"\[Build: ([0-9]+)\]" [ draftBuildNumber ] ->
                SetBuildNumber (sprintf "%s.%s" release.AssemblyVersion draftBuildNumber)
            | _ -> failwithf "Unable to locate build number in release note."

    return draft'
}

// --------------------------------------------------------------------------------------
// Pre-flight checks
// --------------------------------------------------------------------------------------

Target "Checks" (fun _ ->
    let releaseActive = CurrentTargetOrder |> List.concat |> List.exists ((=) "Release")

    if releaseActive then
        Git.CommandHelper.getGitResult "" "fetch --tags"
        |> Log "Fetched tags: "

        let associatedTags = Git.CommandHelper.getGitResult "" "describe --exact-match --tags HEAD"
        let hasTagAssociated = not (associatedTags |> Seq.isEmpty)
        let isReleaseNoteCommit = Git.CommandHelper.getGitResult "" "diff-tree --no-commit-id --name-only -r HEAD" |> Seq.exists ((=) "RELEASE_NOTES.md")

        if (not isReleaseNoteCommit) && (not hasTagAssociated) then
            failwith "You can only release on a tagged or releasable commit."

        if (hasTagAssociated) && (not (hasBuildParam "ForceBuild")) then
            trace (sprintf "Downloading assets for release %s" (associatedTags |> Seq.head))

            CleanDirs [deployDir]

            createGHEClientWithToken gitApi (getBuildParam "GithubToken")
            |> getReleaseByTag gitOwner gitName (associatedTags |> Seq.head)
            |> setBuildNumberFromDraft
            |> downloadAssets deployDir
            |> Async.RunSynchronously

            exit(0)

        if not (hasBuildParam "GithubToken") then
            failwith "You must pass build parameter 'GithubToken' when making a release."
)

// --------------------------------------------------------------------------------------
// Clean build results
// --------------------------------------------------------------------------------------

Target "Clean" (fun _ ->
    CleanDirs [buildAppDir; buildTestDir; deployDir]
)

Target "CleanDocs" (fun _ ->
    CreateDir buildAppDir
    CleanDirs ["docs/output"]
)

// --------------------------------------------------------------------------------------
// Embed version information
// --------------------------------------------------------------------------------------
let GenerateAssemblyInfo moniker file =
    CreateCSharpAssemblyInfo file
        [Attribute.Product "MagiQL"
         Attribute.Version release.AssemblyVersion
         Attribute.FileVersion release.AssemblyVersion
         Attribute.InformationalVersion release.NugetVersion
         Attribute.Company "Salesforce"]

Target "WriteAssemblyInfo" (fun _ ->
    GenerateAssemblyInfo "Tests" "./src/CommonTestAssemblyInfo.cs"
    GenerateAssemblyInfo "Tests" "./src/MagiQLAssemblyInfo.cs"
)

// --------------------------------------------------------------------------------------
// Build application
// --------------------------------------------------------------------------------------

Target "BuildApp" (fun _ ->
    MSBuildWithCustomTransform buildAppDir appReferences
)

// --------------------------------------------------------------------------------------
// Build tests
// --------------------------------------------------------------------------------------

Target "BuildTest_Unit" (fun _ ->
    MSBuildWithCustomTransform buildTestDir unitTestReferences
)

Target "BuildTest_Other" (fun _ ->
    MSBuildWithCustomTransform buildTestDir otherTestReferences
)

Target "BuildTest" DoNothing

// --------------------------------------------------------------------------------------
// Discover and run tests
// --------------------------------------------------------------------------------------

Target "NUnitTest_Unit" (fun _ ->
    !! (buildTestDir + "/**/*.Tests.Unit.dll")
    ++ (buildTestDir + "/**/*.Tests.Unit.*.dll")
        |> NUnit3 (fun p ->
            {p with
                ShadowCopy = false;
                TeamCity = not isLocalBuild;
                Where = "cat != Manual"; 
                ResultSpecs = [buildTestDir @@ "Rules.NUnitTestResults.Unit.xml"];
                Workers = (Some 1);
                ProcessModel = SingleProcessModel;
            })
)

Target "NUnitTest_Other" (fun _ ->
    let callNUnit = (fun _ ->
        !! (buildTestDir + "/**/*.Tests.dll")
        ++ (buildTestDir + "/**/*.Tests.*.dll")
        ++ (buildTestDir + "/**/*.Tests.*.*.dll")
        ++ (buildTestDir + "/**/**/**/*.Tests.*.*.dll")
        ++ (buildTestDir + "/**/**/**/**/*.Tests.*.*.dll")
        ++ (buildTestDir + "/**/**/**/**/**/*.Tests.*.*.dll")
        -- (buildTestDir + "/**/*.Tests.Unit.dll")
        -- (buildTestDir + "/**/*.Tests.Unit.*.dll")
        -- (buildTestDir + "/**/*.Tests.Manual.dll")
        -- (buildTestDir + "/**/*.Tests.Manual.*.dll")
            |> NUnit3 (fun p ->
                {p with
                    ShadowCopy = false;
                    TeamCity = not isLocalBuild;
                    Where = "cat != Manual"; 
                    ResultSpecs = [buildTestDir @@ "Rules.NUnitTestResults.Other.xml"];
                    Workers = (Some 1);
                    ProcessModel = SingleProcessModel;
                })
    )

    
    callNUnit()
     
)

Target "NUnitTest" DoNothing

// --------------------------------------------------------------------------------------
// Package and release
// --------------------------------------------------------------------------------------

Target "Package" (fun _ ->
    Paket.Pack (fun p ->
        { p with
            WorkingDir = "src";
            OutputPath = ".." @@ deployDir;
            Version = finalBuildVersion;
            ReleaseNotes = toLines release.Notes;
        });
    
)

Target "ReleaseDocs" (fun _ ->
    let tempDocsDir = "temp/gh-pages"
    CleanDir tempDocsDir
    Repository.cloneSingleBranch "" (gitHome + "/" + gitName + ".git") "gh-pages" tempDocsDir

    Git.CommandHelper.runSimpleGitCommand tempDocsDir "rm . -f -r" |> ignore
    CopyRecursive "docs/output" tempDocsDir true |> tracefn "%A"

    // File.WriteAllText("temp/gh-pages/latest",sprintf "https://" + gitHome + "/" + gitName + "/releases/download/%s/source.zip" release.NugetVersion)
    // File.WriteAllText("temp/gh-pages/stable",sprintf "https://" + gitHome + "/" + gitName + "/releases/download/%s/source.zip" stable.NugetVersion)

    StageAll tempDocsDir
    Git.Commit.Commit tempDocsDir (sprintf "Update generated documentation for version %s." release.NugetVersion)
    Branches.push tempDocsDir
)

Target "ReleaseGitHub" (fun _ ->
    let releaseTagExists =
        Git.CommandHelper.getGitResult "" "tag"
        |> Seq.exists ((=) release.NugetVersion)

    if not releaseTagExists then
        let remote =
            Git.CommandHelper.getGitResult "" "remote -v"
            |> Seq.filter (fun (s: string) -> s.EndsWith("(push)"))
            |> Seq.tryFind (fun (s: string) -> s.Contains(gitOwner + "/" + gitName))
            |> function None -> gitHome + "/" + gitName | Some (s: string) -> s.Split().[0]

        StageAll ""
        Git.Commit.Commit "" (sprintf "Bump version to %s." release.NugetVersion)
        Branches.pushBranch "" remote (Information.getBranchName "")

        Branches.tag "" release.NugetVersion
        Branches.pushTag "" remote release.NugetVersion

        System.Threading.Thread.Sleep(2000) // wait for the tag to settle in (can't release note until tag exists)

        let releaseFiles =
            !! "deploy/**/*.nupkg"

        createGHEClientWithToken gitApi (getBuildParam "GithubToken")
        |> createDraft gitOwner gitName release.NugetVersion (release.SemVer.PreRelease <> None) (seq { yield! release.Notes; yield "\n`[Build: " + buildNumber + "]`"; yield "`[JIRA: ]`"; })
        |> uploadFiles releaseFiles
        |> releaseDraft
        |> Async.RunSynchronously
    else
        trace (sprintf "Tag %s aleady exists, skipping release tag and note." release.NugetVersion)
)

Target "Bump" DoNothing

Target "Release" DoNothing

// --------------------------------------------------------------------------------------
// Generate documentation
// --------------------------------------------------------------------------------------

Target "GenerateReferenceDocs" (fun _ ->
    if not <| executeFSIWithArgs "docs/tools" "generate.fsx" ["--define:RELEASE"; "--define:REFERENCE"] [] then
      failwith "generating reference documentation failed"
)

let generateHelp' commands fail debug =
    let args =
        [ if not debug then yield "--define:RELEASE"
          if commands then yield "--define:COMMANDS"
          yield "--define:HELP"]

    if executeFSIWithArgs "docs/tools" "generate.fsx" args [] then
        traceImportant "Help generated"
    else
        if fail then
            failwith "generating help documentation failed"
        else
            traceImportant "generating help documentation failed"

let generateHelp commands fail =
    generateHelp' commands fail false

Target "GenerateHelp" (fun _ ->
    DeleteFile "docs/content/release-notes.md"
    CopyFile "docs/content/" "RELEASE_NOTES.md"
    Rename "docs/content/release-notes.md" "docs/content/RELEASE_NOTES.md"

    DeleteFile "docs/content/license.md"
    CopyFile "docs/content/" "LICENSE.txt"
    Rename "docs/content/license.md" "docs/content/LICENSE.txt"

    generateHelp true true
)

Target "GenerateHelpOnEdit" (fun _ ->
    use watcher = !! "docs/content/**/*.*" ++ "docs/tools/templates/*.*" |> WatchChanges (fun changes ->
         generateHelp false false
    )

    traceImportant "Waiting for help edits. Press any key to stop."

    System.Console.ReadKey() |> ignore

    watcher.Dispose()
)

Target "GenerateDocs" DoNothing

// Build order
"Checks"
  ==> "Clean"
  ==> "BuildApp"
  ==> "NUnitTest"
  ==> "Package"

"Package"
  ==> "ReleaseGitHub"
  ==> "Release"

"WriteAssemblyInfo"
  ==> "Bump"

"NUnitTest_Unit"
  ==> "NUnitTest_Other"
  ==> "NUnitTest"

"BuildTest_Unit"
  ==> "BuildTest_Other"
  ==> "BuildTest"

"BuildTest_Unit"
  ==> "NUnitTest_Unit"

"BuildTest_Other"
  ==> "NUnitTest_Other"

"WriteAssemblyInfo"
  ==> "BuildApp"

"CleanDocs"
  ==> "GenerateHelp"
  ==> "GenerateReferenceDocs"
  ==> "GenerateDocs"

"GenerateDocs"
  ==> "ReleaseDocs"

// start build
RunTargetOrDefault "Package"

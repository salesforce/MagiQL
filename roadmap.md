TODO 
====

### Required
- ~Api routes in separate assembly self register~
- ~Client has no dependancies - linked classes~
- ~Remove entity framework~ 
- ~Sql only, don't execute option.~
- ~remove need for Scenario2ReportColumnMappingValidator~
- ~Constants uses unique name not col Id -> have uniquename and ID, set uniquename then lookup the id in a backing field.~
- ~move structuremap configuration to separate assembly.~

- cleanup how Constants() is initialised


- Rename all Tests to be Test.Unit or Test.Integration
- Document how to get scenario tests running
- Change Namespaces
- Remove old fields in mapping table
- remove can group by option in column ui
- add description in column ui
- Rebuild calculated columns to support multiple aggregations on same field


### Nice to Have
- Json configuration 0 code
- Remove structure map
- Configure via registry/configurator
- Make Date selector more dynamic - user defined list of date range types - how to translate that? does Type => column?
- Temporal aggregation -> time breakdowns
- Validation tests - unit tests on the query validators
- separate repository & build scripts to publish nuget packages


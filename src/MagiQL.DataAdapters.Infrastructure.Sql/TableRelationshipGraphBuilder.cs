using System;
using System.Collections.Generic;
using System.Linq;
using MagiQL.DataAdapters.Infrastructure.Sql.Model.TableMapping;

namespace MagiQL.DataAdapters.Infrastructure.Sql
{
    public class TableRelationshipGraph
    {
        public TableRelationshipGraphType Type { get; set; }
        public string TableName { get; set; }
        public int DistanceFromRoot { get; set; }
        public List<TableRelationshipGraph> Relations { get; set; }

        public TableRelationshipGraph()
        {
            Relations = new List<TableRelationshipGraph>();
        }
    }

    public enum TableRelationshipGraphType
    {
        Root,
        Parent,
        Sibling,
        Child
    }


    public class TableRelationshipGraphBuilder
    {
        public TableRelationshipGraph Build(List<TableRelationship> tableRelationships, string rootTableName)
        {
            var foundTables = new List<string>() { rootTableName };
            var foundRelations = new List<TableRelationship>();

            var node = new TableRelationshipGraph()
            {
                TableName = rootTableName,
                Type = TableRelationshipGraphType.Root,
                DistanceFromRoot = 0
            };

            AddRelationsRecursive(tableRelationships, rootTableName, foundRelations, node, foundTables);

            return node;
        }

        private static void AddRelationsRecursive(
            List<TableRelationship> tableRelationships,
            string rootTableName,
            List<TableRelationship> foundRelations,
            TableRelationshipGraph node,
            List<string> foundTables)
        {
            var relations = tableRelationships.Where(x => x.Table1.KnownTableName == rootTableName
                                                          || x.Table2.KnownTableName == rootTableName)
                                              .Where(x => x.IsDirect)
                                              .Where(x => !foundRelations.Contains(x));

            int distance = node.DistanceFromRoot + 1;

            foreach (var r in relations)
            {
                bool table1IsRoot = r.Table1.KnownTableName == rootTableName;
                var relatedTable = table1IsRoot ? r.Table2.KnownTableName : r.Table1.KnownTableName;
                foundTables.Add(relatedTable);
                foundRelations.Add(r);

                TableRelationshipGraphType type;
                switch (r.RelationshipType)
                {
                    case TableRelationshipType.OneToOne:
                        type = TableRelationshipGraphType.Sibling;
                        break;
                    case TableRelationshipType.OneToMany:
                        type = table1IsRoot ? TableRelationshipGraphType.Child : TableRelationshipGraphType.Parent;
                        break;
                    case TableRelationshipType.ManyToOne:
                        type = table1IsRoot ? TableRelationshipGraphType.Parent : TableRelationshipGraphType.Child;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var rNode = new TableRelationshipGraph()
                {
                    DistanceFromRoot = distance,
                    TableName = relatedTable,
                    Type = type
                };

                node.Relations.Add(rNode);
            }

            foreach (var n in node.Relations)
            {
                AddRelationsRecursive(tableRelationships, n.TableName, foundRelations, n, foundTables);
            }

        }

        public void TrimToTables(TableRelationshipGraph relationshipGraph, List<string> tables)
        {
            // remove nodes which dont match table names or have any relations
            relationshipGraph.Relations = relationshipGraph.Relations.Where(x => tables.Contains(x.TableName)
                                                                              || x.Relations.Any()
            ).ToList();

            foreach (var r in relationshipGraph.Relations)
            {
                TrimToTables(r, tables); // remove table name
            }

            // cleanup again
            relationshipGraph.Relations = relationshipGraph.Relations.Where(x => tables.Contains(x.TableName)
                                                                              || x.Relations.Any()
            ).ToList();

        }

        public List<string> GetAllTables(TableRelationshipGraph relationshipGraph)
        {
            var result = new List<string>();

            GetAllTablesRecursive(relationshipGraph, result);

            return result.Distinct().ToList();
        }

        private void GetAllTablesRecursive(TableRelationshipGraph relationshipGraph,List<string> result)
        {
            result.Add(relationshipGraph.TableName);

            foreach (var r in relationshipGraph.Relations)
            {
                GetAllTablesRecursive(r, result);
            }
        }

        public int GetDistance(TableRelationshipGraph relationshipGraph, string tableName, int defaultValue = 999)
        { 
            var table = FindTable(relationshipGraph, tableName);

            return table!=null ? table.DistanceFromRoot : defaultValue;
        }

        public TableRelationshipGraph FindTable(TableRelationshipGraph relationshipGraph, string tableName)
        {
            if (relationshipGraph.TableName == tableName)
            {
                return relationshipGraph;
            }
            foreach (var r in relationshipGraph.Relations)
            {
                var result = FindTable(r, tableName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }


        public List<TableRelationshipGraph> GetByDistance(TableRelationshipGraph relationshipGraph, int distance)
        {
            var result = new List<TableRelationshipGraph>();

            GetByDistanceRecursive(relationshipGraph, distance, result);
            
            return result;
        }

        private void GetByDistanceRecursive(TableRelationshipGraph relationshipGraph,int distance,List<TableRelationshipGraph> result)
        {
            if (relationshipGraph.DistanceFromRoot == distance)
            {
                result.Add(relationshipGraph);
            }
            else if (relationshipGraph.DistanceFromRoot < distance)
            {
                foreach (var r in relationshipGraph.Relations)
                {
                    GetByDistanceRecursive(r, distance, result);
                }
            }
        }

        public bool TablesAreInSamePathToRoot(TableRelationshipGraph relationshipGraph, string table1, string table2)
        {
            // get the path from table1 to relationshipGraph root table
            var path1 = GetPathFromTableToRoot(relationshipGraph, table1); 

            return path1.Contains(table2);
        }

        public List<string> GetPathFromTableToRoot(TableRelationshipGraph relationshipGraph,string tableName)
        {
            var distance = GetDistance(relationshipGraph, tableName);

            var path = new List<string> {tableName};
             
            string currentTableName = tableName;
            while (distance > 0)
            {
                var potentialNodes = GetByDistance(relationshipGraph, distance - 1);
                var parentNode = potentialNodes.SingleOrDefault(x => x.Relations.Any(y => y.TableName == currentTableName));
                if (parentNode == null)
                {
                    throw new Exception(string.Format("Could not find a relationship between {0} and {1}", currentTableName, relationshipGraph.TableName));
                } 
                currentTableName = parentNode.TableName;
                distance--;
                path.Add(parentNode.TableName);
            }

            return path;
        }
    }
}

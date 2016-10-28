using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using DapperExtensions;
using MagiQL.Framework.Interfaces.Repository;
using MagiQL.Framework.Model.Columns;

namespace MagiQL.Framework.Repositories.Repositories
{
    public class ReportColumnMappingRepository : ReportsRepository<ReportColumnMapping>, IReportColumnMappingRepository
    {
        public override IEnumerable<ReportColumnMapping> QueryWhere(string whereClause, object parameters)
        {
            var result = base.QueryWhere(whereClause, parameters).ToList();

            LoadMetaData(whereClause, parameters, result);

            return result;
        }

        private void LoadMetaData(string whereClause, object parameters, List<ReportColumnMapping> result)
        {
            if (result.Any())
            {
                var metaDatas =
                    Connection.Query<ReportColumnMetaDataValue>(
                        "SELECT ReportColumnMappingMetaData.* FROM ReportColumnMappingMetaData LEFT JOIN ReportColumnMapping  "
                            + "on ReportColumnMapping.ID = ReportColumnMappingMetaData.ReportColumnMappingId WHERE " 
                            + whereClause, parameters);

                if (metaDatas.Any())
                {
                    var metaDatasGrouped = metaDatas.GroupBy(x => x.ReportColumnMappingId);
                    foreach (var md in metaDatasGrouped)
                    {
                        result.Single(x => x.Id == md.Key).MetaData = md.ToList();
                    }
                }
            }
        }

        public IList<ReportColumnMapping> GetReportColumnMappingsById(int dataSourceTypeId, IEnumerable<int> columnIds)
        {
            string whereClause = string.Format("DataSourceTypeId = @dataSourceTypeId AND ReportColumnMapping.Id in ({0})", string.Join(",", columnIds));

            var parameters = new { dataSourceTypeId };

            var result = QueryWhere(whereClause, parameters).ToList();

            return result;
        }
        
        public ReportColumnMapping GetReportColumnMapping(int id)
        {
            string whereClause = string.Format("ReportColumnMapping.Id = @id");

            var parameters = new { id };

            var result = QueryWhere(whereClause, parameters).SingleOrDefault();

            return result;
        }

        public void Delete(int id)
        {
            var entity = GetReportColumnMapping(id);
            using (var scope = CreateTransaction())
            {
                Delete(entity, scope);
                scope.Commit();
            }
        }

        public IList<ReportColumnMapping> GetReportColumnMappingsByOrganizationId(int dataSourceTypeId, int? organizationId)
        {
            string whereClause = "DataSourceTypeId = @dataSourceTypeId AND " +
                                 ((organizationId == null) ? "OrganizationId IS NULL" : "OrganizationId = @organizationId"); 

            var parameters = new {dataSourceTypeId, organizationId};

            var result = QueryWhere(whereClause, parameters).ToList();

            return result;
        }
         
        public IList<ReportColumnMapping> Find(int dataSourceTypeId, string table, string field, int? actionSpecId)
        {
            string whereClause = "m.DataSourceTypeId = @dataSourceTypeId "
                                + " AND  KnownTable == @table"
                                + " AND  FieldName == @field"
                                + (actionSpecId == null ? " AND  ActionSpecId IS NULL"  : " AND  ActionSpecId == @actionSpecId")
                                + " AND  CreatedByUserId IS null"
                                + " AND  IsCalculated == 0";

            var parameters = new { dataSourceTypeId, table, field, actionSpecId };

            var result = QueryWhere(whereClause, parameters).ToList();  

            return result;
        }

        public IList<ReportColumnMapping> Find(int dataSourceTypeId, string uniqueName)
        {

            string whereClause = "DataSourceTypeId = @dataSourceTypeId AND UniqueName = @uniqueName";

            var parameters = new { dataSourceTypeId, uniqueName };

            var result = QueryWhere(whereClause, parameters).ToList(); 

            return result;
        }

        public override ReportColumnMapping Add(ReportColumnMapping entity, IDbTransaction scope)
        {
            var result = base.Add(entity, scope);

            if (result != null)
            {                
                SetMetaData(entity, scope, entity);
            }

            return result;
        }

        public void Update(ReportColumnMapping entity, IDbTransaction scope)
        {
            var original = GetReportColumnMapping(entity.Id);

            base.Update(entity, scope);

            SetMetaData(entity, scope, original);
        }

        private void SetMetaData(ReportColumnMapping entity, IDbTransaction scope, ReportColumnMapping original)
        {
            GetConnection(scope)
                .Execute(
                    "DELETE FROM ReportColumnMappingMetaData WHERE ReportColumnMappingID = @reportColumnMappingId",
                    new {reportColumnMappingId = original.Id},
                    scope);

            if (entity.MetaData.Any())
            {
                // todo : inspect the metadata values to see if anything has changed

                foreach (var md in entity.MetaData.Where(x => x.ReportColumnMappingId == 0))
                {
                    md.ReportColumnMappingId = entity.Id;
                }

                foreach (var md in entity.MetaData.Where(x => x.ReportColumnMappingId == entity.Id))
                {
                    GetConnection(scope).Insert(md, scope);
                }
            }
        }
    }
}

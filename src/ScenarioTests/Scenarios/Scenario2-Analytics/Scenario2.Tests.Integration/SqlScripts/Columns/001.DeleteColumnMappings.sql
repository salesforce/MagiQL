delete reportcolumnmappingmetadata 
from reportcolumnmappingmetadata 
inner join reportcolumnmapping on reportcolumnmapping.id = reportcolumnmappingmetadata.reportcolumnmappingid 
where reportcolumnmapping.datasourcetypeid = 2

delete from reportcolumnmapping where datasourcetypeid = 2
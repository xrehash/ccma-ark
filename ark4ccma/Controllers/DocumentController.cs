using Dapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace ark4ccma.Controllers
{
  [Produces("application/json")]
  [Route("api/ccmadb")]
  
  public class DocumentController : Controller
  {
    private readonly SqlConnection _connection;

    public DocumentController(
           )
    {
      _connection = new SqlConnection(@"server=.\dblocks;database=Box;user id=noah;password=ccmauser;");
    }

    [HttpGet]
    public List<dynamic> GetAll(
                         )
    {
      return _connection.Query<dynamic>("select Id,ApplicationNumber,DocumentUNID,FormName from ark.DocumentTable where  ApplicationNumber <> '' and FormName like '%Application%'")
        .ToList<dynamic>();
    }

    [HttpGet("unid/{unidDoc}")]
    public dynamic GetDocument(
                   string unidDoc)
    {
      dynamic doc = _connection.Query<dynamic>(
                      @"SELECT d.[Id],d.[DocumentTableID],d.[ApplicationNumber],d.[FormName]
                            ,d.[FieldName]
                            ,d.[FieldValue]
                        FROM [ark].[DocumentDataTable] d inner join ark.DocumentTable t on d.DocumentTableID = t.Id
                        Where t.DocumentUNID = @docunid",
                      new { docunid = unidDoc });

      return doc;
    }

    [HttpGet("application-header/{app}")]
    public dynamic GetApplicationDocuments(
                   string app)
    {
      return _connection.Query<dynamic>(
               "select Id,ApplicationNumber,DocumentUNID,FormName from ark.DocumentTable where  ApplicationNumber = @appNo",
               new { appNo = app })
        .ToList<dynamic>();
    }

    [HttpGet("application/{appno}")]
    public dynamic GetApplication(
                   string appno)
    {
      dynamic appData = _connection.Query<dynamic>(
                          @"SELECT d.[Id],d.[DocumentTableID],d.[ApplicationNumber],d.[FormName]
      ,d.[FieldName]
      ,d.[FieldValue]
      FROM [ark].[DocumentDataTable] d inner join ark.DocumentTable t on d.DocumentTableID = t.Id
      Where t.ApplicationNumber = @aNo",
                                                   new { aNo = appno });
      return appData;
    }

    [HttpGet("search/{term}")]
    public List<dynamic> GetSearchResults(
                         string term)
    {
      var lst = _connection.Query<dynamic>(
               @"
select d.ApplicationNumber,d.FormName,d.score, DocumentUNID from
(SELECT top 100 [ApplicationNumber]
      ,[FormName]
      ,sum(case 
			when [FormName] = 'Application' then 5 
			when [FormName] = 'ArkApplications' then 4
			when [FormName] = 'frmResponse' then 0.75 else 1 end) score
  FROM [ark].[DocumentDataTable]
  Where lower(FieldValue) like @SearchTerm
  group by ApplicationNumber,FormName) d inner join [ark].[DocumentTable] t on d.ApplicationNumber = t.ApplicationNumber and d.FormName = t.FormName
  order by 3 desc, 1 desc",
                                        new { SearchTerm = $"%{term}%" })
        .ToList<dynamic>();
      return lst;
    }
  }
}
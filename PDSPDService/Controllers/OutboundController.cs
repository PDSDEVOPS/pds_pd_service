using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Web;
using Microsoft.AspNet.Identity;
using System.IO;
using System.Diagnostics;
using log4net;
using System.Text.RegularExpressions;
using ProductionWebService.Parsers;
using ProductionWebService.Utilities;
using ProductionWebService.Services;
using ProductionWebService.Persistence;
using ProductionWebService.Models;
using ProductionWebService.Providers;
using ProductionWebService.Helpers;
using ProductionWebService.Security;
using System.Data;
using System.Xml;

namespace ProductionWebService.Controllers
{
    //[Authorize]
    [OverrideAuthentication]
    [OutboundServicesAuthorizeAttribute]
    [UserIsValidAuthorizationAttribute]
    [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
    [RoutePrefix("api/Outbound")]
    public class OutboundController : ApiController
    {
        private ILog log = log4net.LogManager.GetLogger(typeof(OutboundController));
        private const string APPLICATION_XML = @"application/xml";

        [HttpGet]
        public IHttpActionResult GetProductionDailyDto(DateTime? StartDate = null, DateTime? EndDate = null)
        {
            return GetProductionDtoCommon(StartDate, EndDate);
        }

        [HttpGet]
        public IHttpActionResult GetProductionMonthlyDto(DateTime? StartDate = null, DateTime? EndDate = null)
        {
            return GetProductionDtoCommon(StartDate, EndDate);
        }

        private IHttpActionResult GetProductionDtoCommon(DateTime? StartDate = null, DateTime? EndDate = null)
        {
            UserRouteQueryParameter QueryParams = null;

            QueryParams = GetUserRouteQueryParameters();

            if( QueryParams != null)
            {
                var Table = GetDataTable(QueryParams);

                var TableName = GetDataTableName();
                Table.TableName = TableName;

                var ds = new DataSet();
                ds.DataSetName = GetDataSetName();
                ds.Tables.Add(Table);

                var RequestHeader = RequestHelper.GetApplicationReturnType(ActionContext);
                if (RequestHeader != null && RequestHeader.Trim().ToLower() == APPLICATION_XML)
                {
                    return Ok(GetXML(Table));
                }
                else
                {
                    return Ok(Table);
                }
            }
            else
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetProductionDailyBinary(DateTime? StartDate = null, DateTime? EndDate = null)
        {
            return GetProductionBinaryCommon(StartDate, EndDate);
        }

        [HttpGet]
        public HttpResponseMessage GetProductionMonthlyBinary(DateTime? StartDate = null, DateTime? EndDate = null)
        {
            return GetProductionBinaryCommon(StartDate, EndDate);
        }

        public HttpResponseMessage GetProductionBinaryCommon(DateTime? StartDate = null, DateTime? EndDate = null)
        {
            HttpResponseMessage result = null;

            UserRouteQueryParameter QueryParams = null;

            QueryParams = GetUserRouteQueryParameters();

            if( QueryParams != null)
            {
                var ReturnFile = GetFile(QueryParams);

                result = new HttpResponseMessage(HttpStatusCode.OK);
                Stream stream = new MemoryStream(ReturnFile);
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                return result;
            }
            else
            {
                return new HttpResponseMessage( HttpStatusCode.Unauthorized);
            }

            
        }

        // Stubbed out so we can call a SP, etc later.
        private DataTable GetDataTable(UserRouteQueryParameter QueryParams)
        {
            var ReturnFile = GetFile(QueryParams);

            var CSVParser = new CSVFileParser();
            var ListOfArrays = CSVParser.ReadCSV(ReturnFile);

            var ConvertionUtility = new Utility();

            var Table = ConvertionUtility.GetDataTableFromStringArrays(ListOfArrays, QueryParams.AdditionalParameters, true);

            return Table;
        }

        private XmlElement GetXML(DataTable Table)
        {
            var ReturnValue = ConvertDatatableToXML(Table);
            var XML = new XmlDocument();
            XML.LoadXml(ReturnValue);
            return XML.DocumentElement;
        }

        public string ConvertDatatableToXML(DataTable dt)
        {
            MemoryStream str = new MemoryStream();
            dt.WriteXml(str, true);
            str.Seek(0, SeekOrigin.Begin);
            StreamReader sr = new StreamReader(str);
            string xmlstr;
            xmlstr = sr.ReadToEnd();
            return (xmlstr);
        }

        private string GetDataSetName()
        {
            const string DEFAULT_DS_NAME = "ProductionData";
            string DSName = DEFAULT_DS_NAME;

            return DSName;
        }
        private string GetDataTableName()
        {
            const string DEFAULT_TABLE_NAME = "ProductionDataDto";
            string TableName = DEFAULT_TABLE_NAME;

            return TableName;
        }

        private Byte[] GetFile(UserRouteQueryParameter QueryParams)
        {
            FtpService FTPServ = new FtpService(QueryParams.Host, QueryParams.UserName, QueryParams.Password, false, FTPType.FTP);
            var FileReturn = FTPServ.DownloadLatestFileWithExtFTP(QueryParams.Folder, QueryParams.FileMatch);

            return FileReturn;
        }

        private UserRouteQueryParameter GetUserRouteQueryParameters()
        {
            UserRouteQueryParameter QueryParams = null;

            var UserId = RequestHelper.GetUserId(ActionContext);
            var RouteUrl = String.Format("/{0}/", RouteHelper.GetControllActionRoute(ActionContext));// RouteHelper.GetControllActionRoute(ActionContext);

            using( var db = new ApplicationDbContext())
            {
                var ExRouteAccess = db.ExSysRouteAccesses.Where(x => x.UserId == UserId && x.RouteUrl.ToLower() == RouteUrl.ToLower()).FirstOrDefault();

                if( ExRouteAccess != null)
                {
                    QueryParams = db.UserRouteQueryParameters.Where(x => x.ExSysRouteAccessId == ExRouteAccess.Id).FirstOrDefault();
                }
            }

            return QueryParams;
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using GatherContent.Connector.Managers.Managers;
using GatherContent.Connector.Managers.Models.ImportItems;
using Sitecore.Diagnostics;
using Sitecore.Services.Infrastructure.Web.Http;

namespace GatherContent.Connector.Website.Controllers
{
    public class ImportController : ServicesApiController
    {
        public HttpResponseMessage Get(string id, string projectId)
        {
            HttpResponseMessage response;
            try
            {
                var importManager = new ImportManager();
                SelectItemsForImportModel result = importManager.GetModelForSelectImportItemsDialog(id, projectId);
                response = Request.CreateResponse(HttpStatusCode.OK, result);
                return response;
            }
            catch (Exception exception)
            {
                Log.Error(exception.Message, exception);
                response = Request.CreateResponse(HttpStatusCode.InternalServerError, exception.Message);
                return response;
            }
        }

        public HttpResponseMessage ImportItems(string id, string projectId, string statusId, List<ImportListItem> items)
        {
            HttpResponseMessage response;
            try
            {
                var importManager = new ImportManager();
                ImportResultModel result = importManager.ImportItems(id, items, projectId, statusId);
                response = Request.CreateResponse(HttpStatusCode.OK, result);
                return response;
            }
            catch (Exception exception)
            {
                Log.Error(exception.Message, exception);
                response = Request.CreateResponse(HttpStatusCode.InternalServerError, exception.Message);
                return response;
            }
        }
    }
}
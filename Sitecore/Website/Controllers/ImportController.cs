﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Helpers;
using System.Web.Mvc;
using GatherContent.Connector.Managers.Managers;
using GatherContent.Connector.Managers.Models.ImportItems;
using Newtonsoft.Json;
using Sitecore.Diagnostics;
using Sitecore.Mvc.Controllers;
using Sitecore.Services.Infrastructure.Web.Http.Formatting;

namespace GatherContent.Connector.Website.Controllers
{
    public class ImportController : SitecoreController
    {
        private readonly ImportManager _importManager;

        public ImportController()
        {
            _importManager = new ImportManager();
        }


        public string Get(string id, string projectId)
        {
            try
            {
                SelectItemsForImportModel result = _importManager.GetModelForSelectImportItemsDialog(id, projectId);
                var model = JsonConvert.SerializeObject(result);
                return model;
            }
            catch (Exception exception)
            {
                Log.Error(exception.Message, exception);
            }
            return null;
        }



        public string ImportItems(string id, string projectId, string statusId, List<ImportListItem> items)
        {
            try
            {
                ImportResultModel result = _importManager.ImportItems(id, items, projectId, statusId);
                var model = JsonConvert.SerializeObject(result);
                return model;
            }
            catch (Exception exception)
            {
                Log.Error(exception.Message, exception);
            }
            return null;
        }

    }
}
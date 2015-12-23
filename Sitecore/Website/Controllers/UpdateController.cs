﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;
using GatherContent.Connector.Managers.Managers;
using GatherContent.Connector.Managers.Models.UpdateItems;
using Newtonsoft.Json;
using Sitecore.Diagnostics;
using Sitecore.Mvc.Controllers;

namespace GatherContent.Connector.Website.Controllers
{
    public class UpdateController : SitecoreController
    {
        private readonly UpdateManager _updateManager;

        public UpdateController()
        {
            _updateManager = new UpdateManager();
        }

        public string Get(string id)
        {
            try
            {
                SelectItemsForUpdateModel result = _updateManager.GetItemsForUpdate(id);
                var model = JsonConvert.SerializeObject(result);
                return model;
            }
            catch (Exception exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
            }
            return null;
        }

        public ActionResult UpdateItems(string id, string statusId, List<UpdateListIds> items)
        {
            try
            {
                UpdateResultModel result = _updateManager.UpdateItems(id, items);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                return Json(new { status = "error", message = exception.Message }, JsonRequestBehavior.AllowGet);
            }
            return null;
        }
    }
}
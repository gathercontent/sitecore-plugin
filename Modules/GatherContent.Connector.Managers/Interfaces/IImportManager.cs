﻿using System.Collections.Generic;
using GatherContent.Connector.Entities.Entities;
using GatherContent.Connector.Managers.Models.ImportItems;
using GatherContent.Connector.Managers.Models.ImportItems.New;

namespace GatherContent.Connector.Managers.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IImportManager : IManager
    {
        SelectItemsForImportWithLocation GetDialogModelWithLocation(string itemId, string projectId);

        List<ItemResponseModel> ImportItems(string itemId, List<ImportItemModel> items, string projectId, string statusId, string language);

        ImportResultModel ImportItemsWithLocation(List<LocationImportItemModel> items, string projectId, string statusId, string language);

        SelectItemsForImportModel GetModelForSelectImportItemsDialog(string itemId, string projectId);

        List<MappingResultModel> MapItems(List<GCItem> items);
    }
}
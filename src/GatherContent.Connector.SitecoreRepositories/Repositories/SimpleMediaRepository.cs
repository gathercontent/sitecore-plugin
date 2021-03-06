﻿using System.IO;
using System.Linq;
using System.Net;
using GatherContent.Connector.IRepositories.Interfaces;
using GatherContent.Connector.IRepositories.Models.Import;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;
using Sitecore.SecurityModel;
using File = GatherContent.Connector.IRepositories.Models.Import.File;
using GatherContent.Connector.GatherContentService.Services;

namespace GatherContent.Connector.SitecoreRepositories.Repositories
{
    public class SimpleMediaRepository : BaseSitecoreRepository, IMediaRepository<Item>
    {
        public Item UploadFile(string targetPath, File fileInfo)
        {

            var gcsettings = new AccountsRepository().GetAccountSettings();
            var itemService = new ItemsService(gcsettings);

            string extension = string.Empty;
            if (fileInfo.FileName.Contains("."))
            {
                extension = fileInfo.FileName.Substring(fileInfo.FileName.LastIndexOf('.') + 1);
            }

            var memoryStream = itemService.DownloadFile(fileInfo.FileId) as MemoryStream;
            try
            {
                if (memoryStream.Length > 0)
                {
                    var media = CreateMedia(targetPath, fileInfo, extension, memoryStream);
                    return media;
                }
            }
            finally
            {
                memoryStream.Close();
            }

            return null;
        }

        public virtual string ResolveMediaPath(CmsItem item, Item createdItem, CmsField cmsField)
        {
            Field scField = createdItem.Fields[new ID(cmsField.TemplateField.FieldId)];
            string dataSourcePath = GetItem(scField.ID.ToString())["Source"];
            string path;
            if (!string.IsNullOrEmpty(dataSourcePath) && GetItem(dataSourcePath) != null)
            {
                path = dataSourcePath;
            }
            else
            {
                path = string.IsNullOrEmpty(cmsField.TemplateField.FieldName)
                    ? string.Format("/sitecore/media library/GatherContent/{0}/", ItemUtil.ProposeValidItemName(item.Title))
                    : string.Format("/sitecore/media library/GatherContent/{0}/{1}/", ItemUtil.ProposeValidItemName(item.Title), ItemUtil.ProposeValidItemName(cmsField.TemplateField.FieldName));

                SetDatasourcePath(createdItem, cmsField.TemplateField.FieldId, path);
            }

            return path;
        }

        protected virtual Item CreateMedia(string rootPath, File mediaFile, string extension, Stream mediaStream)
        {
            using (new SecurityDisabler())
            {
                var validItemName = ItemUtil.ProposeValidItemName(mediaFile.FileName);

                var filesFolder = GetItemByPath(rootPath);
                if (filesFolder != null)
                {
                    var files = filesFolder.Children;
                    var item = files.FirstOrDefault(f => f.Name == validItemName &&
                                                         DateUtil.IsoDateToDateTime(f.Fields["__Created"].Value) >=
                                                         mediaFile.UpdatedDate);
                    if (item != null)
                    {
                        return item;
                    }
                }

                var mediaOptions = new MediaCreatorOptions
                {
                    Database = ContextDatabase,
                    FileBased = false,
                    IncludeExtensionInItemName = false,
#if SC72 || SC80 || SC81
                    KeepExisting = true, //till sc8.2, in 9.0 removed
#else
                    OverwriteExisting = false, //from sc8.2
#endif
                    Versioned = false,
                    Destination = string.Concat(rootPath, "/", validItemName)
                };

                var previewImgItem = MediaManager.Creator.CreateFromStream(mediaStream, validItemName + "." + extension, mediaOptions);
                return previewImgItem;
            }
        }

        protected void SetDatasourcePath(Item updatedItem, string fieldId, string path)
        {
            var scField = updatedItem.Fields[new ID(fieldId)];
            var scItem = GetItem(scField.ID.ToString());

            if (string.IsNullOrEmpty(scItem["Source"]))
            {
                using (new SecurityDisabler())
                {
                    scItem.Editing.BeginEdit();
                    scItem["Source"] = path;
                    scItem.Editing.EndEdit();
                }
            }
        }
    }
}
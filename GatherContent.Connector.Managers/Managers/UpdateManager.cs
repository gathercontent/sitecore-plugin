﻿using System.Collections.Generic;
using System.Linq;
using GatherContent.Connector.Entities;
using GatherContent.Connector.Entities.Entities;
using GatherContent.Connector.GatherContentService.Services;
using GatherContent.Connector.IRepositories.Models.Import;
using GatherContent.Connector.IRepositories.Models.Update;
using GatherContent.Connector.Managers.Models.ImportItems;
using GatherContent.Connector.Managers.Models.UpdateItems;
using GatherContent.Connector.SitecoreRepositories.Repositories;

namespace GatherContent.Connector.Managers.Managers
{
    public class GCItemWithCMSItemId : GCItem
    {
        public string CMSId { get; set; }

        public GCItemWithCMSItemId(GCItem item, string cmsId)
        {
            this.CMSId = cmsId;
            this.Config = item.Config;
            this.Created = item.Created;
            this.CustomStateId = item.CustomStateId;
            this.ProjectId = item.ProjectId;
            this.DueDates = item.DueDates;
            this.Id = item.Id;
            this.Name = item.Name;
            this.Notes = item.Notes;
            this.Overdue = item.Overdue;
            this.Updated = item.Updated;
            this.Type = item.Type;
            this.ParentId = item.ParentId;
            this.Position = item.Position;
            this.Status = item.Status;
            this.TemplateId = item.TemplateId;
        }
    }

    public class UpdateManager : BaseManager
    {
        private readonly ItemsRepository _itemsRepository;
        private readonly ItemsService _itemsService;
        private readonly ProjectsService _projectsService;
        private readonly TemplatesService _templatesService;
        private readonly GCAccountSettings _gcAccountSettings;
        private readonly MappingManager _mappingManager;

        public UpdateManager()
        {
            _itemsRepository = new ItemsRepository();

            var accountsRepository = new AccountsRepository();
            _gcAccountSettings = accountsRepository.GetAccountSettings();

            _itemsService = new ItemsService(_gcAccountSettings);
            _projectsService = new ProjectsService(_gcAccountSettings);
            _templatesService = new TemplatesService(_gcAccountSettings);

            _mappingManager = new MappingManager();
        }

        public SelectItemsForUpdateModel GetItemsForUpdate(string itemId)
        {
            List<CMSUpdateItem> cmsItems = _itemsRepository.GetItemsForUpdate(itemId);

            List<GCTemplate> templates;
            List<GCStatus> statuses;
            List<UpdateListItem> models;
            TryToGetModelData(cmsItems, out templates, out statuses, out models);
            
            var result = new SelectItemsForUpdateModel(models, statuses, templates);
            return result;
        }

        private bool TryToGetModelData(List<CMSUpdateItem> cmsItems, out List<GCTemplate> templates, out List<GCStatus> statuses, out List<UpdateListItem> items)
        {
            var projects = new Dictionary<int, Project>();
            var templatesDictionary = new Dictionary<int, GCTemplate>();
            
            statuses = new List<GCStatus>();
            items = new List<UpdateListItem>();

            foreach (CMSUpdateItem cmsItem in cmsItems)
            {
                ItemEntity entity = _itemsService.GetSingleItem(cmsItem.GCItemId);
                if (entity != null)
                {
                    GCItem gcItem = entity.Data;
                    Project project = GetProject(projects, gcItem.ProjectId);
                    if (gcItem.TemplateId.HasValue)
                    {
                        GCTemplate template = GetTemplate(templatesDictionary, gcItem.TemplateId.Value);

                        var listItem = new UpdateListItem(gcItem, template, cmsItem, _gcAccountSettings.DateFormat, project.Name);
                        items.Add(listItem);

                        GCStatus status = gcItem.Status.Data;
                        if (statuses.All(i => i.Id != status.Id))
                        {
                            statuses.Add(status);
                        }
                    }

                }
            }
            
            templates = templatesDictionary.Values.ToList();

            return true;
        }

        private GCTemplate GetTemplate(Dictionary<int, GCTemplate> templates, int templateId)
        {
            GCTemplate template;
            templates.TryGetValue(templateId, out template);

            if (template == null)
            {
                template = _templatesService.GetSingleTemplate(templateId.ToString()).Data;
                templates.Add(templateId, template);
            }

            return template;
        }

        private Project GetProject(Dictionary<int, Project> projects, int projectId)
        {
            Project project;
            projects.TryGetValue(projectId, out project);

            if (project == null)
            {
                project = _projectsService.GetSingleProject(projectId.ToString()).Data;
                projects.Add(projectId, project);
            }

            return project;
        }


        public ImportResultModel UpdateItems(string itemId, List<UpdateListItem> models)
        {
            List<GCItem> gcItems = GetGCItemsByModels(models);
            List<ImportItemResponseModel> resultItems = _mappingManager.MapItems(gcItems);
            _itemsRepository.UpdateItems(resultItems);

            var result = new ImportResultModel(resultItems);

            return result;
        }

        private List<GCItem> GetGCItemsByModels(List<UpdateListItem> models)
        {
            var result = new List<GCItem>();

            foreach (var item in models)
            {
                GCItem gcItem = _itemsService.GetSingleItem(item.GCId).Data;
                var gcItemWithCMSId = new GCItemWithCMSItemId(gcItem, item.CMSId);
                result.Add(gcItemWithCMSId);
            }

            return result;
        }
    }
}

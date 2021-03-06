﻿using System;
using System.Collections.Generic;
using System.Linq;
using GatherContent.Connector.IRepositories.Interfaces;
using GatherContent.Connector.IRepositories.Models.Import;
using Sitecore.Configuration;
using Sitecore.Data.Items;

namespace GatherContent.Connector.SitecoreRepositories.Repositories
{
    public class DropTreeRepository : BaseSitecoreRepository, IDropTreeRepository
    {
        private readonly IAccountsRepository _accountsRepository;

        public DropTreeRepository()
        {
            _accountsRepository = Factory.CreateObject("gatherContent.connector/components/accountsRepository", true) as IAccountsRepository;
        }

        private List<CmsItem> CreateChildrenTree(string id, IEnumerable<Item> items)
        {
            var list = new List<CmsItem>();

            if (items.Select(i => i.ID.ToString()).Contains(id))
            {
                foreach (var item in items)
                {
                    var template = GetItemTemplate(item.TemplateID);
                    if (id == item.ID.ToString())
                    {
                        var node = new CmsItem
                        {
                            Title = item.Name,
                            Id = item.ID.ToString(),
                            Icon = template != null ? template.Icon : "",
                        };
                        list.Add(node);
                    }
                    else
                    {
                        var node = new CmsItem
                        {
                            Title = item.Name,
                            Id = item.ID.ToString(),                            
                            Icon = template != null ? template.Icon : "",
                        };
                        list.Add(node);
                    }
                }
            }
            else
            {
                foreach (var item in items)
                {
                    var template = GetItemTemplate(item.TemplateID);

                    var node = new CmsItem
                    {
                        Title = item.Name,
                        Id = item.ID.ToString(),
                        Icon = template != null ? template.Icon : "",
                        Children = CreateChildrenTree(id, item.Children),
                    };
                    list.Add(node);
                }
            }

            return list;
        }

        public CmsItem GetHomeNode(string id)
        {
            CmsItem model = null;
            var accountSettings = _accountsRepository.GetAccountSettings();
            var dropTreeHomeNode = accountSettings.DropTreeHomeNode;
            if (string.IsNullOrEmpty(dropTreeHomeNode))
            {
                dropTreeHomeNode = Constants.DropTreeHomeNode;
            }
            var home = GetItem(dropTreeHomeNode);
            var template = GetItemTemplate(home.TemplateID);

            if (string.IsNullOrEmpty(id) || id == "null")
            {
                model  = new CmsItem
                {
                    Title = home.Name,
                    Id = home.ID.ToString(),
                    Icon = template != null ? template.Icon : "",
                };
            }
            else
            {
                model = new CmsItem
                {
                    Title = home.Name,
                    Id = home.ID.ToString(),
                    Icon = template != null ? template.Icon : "",
                    Children = CreateChildrenTree(id, home.Children),
                };
            }

            return model;
        }

        public List<CmsItem> GetChildren(string id)
        {
            var model = new List<CmsItem>();
            var parent = GetItem(id);
            if (parent == null) return model;

            var children = parent.Children;

            foreach (var child in children)
            {
                var item = (Item)child;
                var template = GetItemTemplate(item.TemplateID);
                model.Add(new CmsItem
                {
                    Title = item.Name,
                    Id = item.ID.ToString(),
                    Icon = template != null ? template.Icon : ""
                });
            }

            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetHomeNodeId()
        {
            var accountSettings = _accountsRepository.GetAccountSettings();

            var dropTreeHomeNode = accountSettings.DropTreeHomeNode;
            if (string.IsNullOrEmpty(dropTreeHomeNode))
            {
                dropTreeHomeNode = Constants.DropTreeHomeNode;
            }

            return dropTreeHomeNode;
        }

        public List<string> GetIdPath(string parentId, string decendantId)
        {
            List<string> results = new List<string>();


            if (!String.IsNullOrEmpty(parentId) && !String.IsNullOrEmpty(decendantId))
            {

                var parentItem = GetItem(parentId);
                var decendantItem = GetItem(decendantId);

                if (parentItem != null && decendantItem != null & decendantItem.Paths.FullPath.Contains(parentItem.Paths.FullPath))
                {
                    Item i = decendantItem;
                    while(i.ID != parentItem.ID){
                        results.Add(i.ID.ToString());
                        i = i.Parent;
                    }
                    results.Reverse();
                }
            }

            return results;
        
        }
    }
}
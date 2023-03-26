﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace RuntimeNodeEditor
{
    public class ContextMenu : MonoBehaviour
    {
        public event Action<ContextItemData, ContextContainer>   OnMenuItemClick;

        public GameObject           contextContainerPrefab;
        public GameObject           contextItemPrefab;
        private RectTransform       _rect;
        private ContextContainer     _root;

        private List<ContextContainer> _subContainers;

        public void Init()
        {
            _rect = this.GetComponent<RectTransform>();
            _subContainers = new List<ContextContainer>();
            OnMenuItemClick += OnMenuItemClicked;
        }

        public void OnMenuItemClicked(ContextItemData data, ContextContainer container)
        {
            List<ContextContainer> toRemove = new List<ContextContainer>();
            foreach (var item in _subContainers)
            {
                if (item.depthLevel > data.Level)
                {
                    toRemove.Add(item);
                }
            }

            foreach (var item in toRemove)
            {
                Destroy(item.gameObject);
                _subContainers.Remove(item);
            }

            _subContainers.Add(container);
        }

        public void Clear()
        {
            if (_root != null)
            {
                Destroy(_root.gameObject);
                _subContainers = new List<ContextContainer>();
            }
        }

        public void Show(ContextItemData context, Vector2 pos)
        {
            _rect.localPosition = pos;
            _root = Instantiate(contextContainerPrefab, _rect).GetComponent<ContextContainer>();
            PopulateContainer(_root, context.children.ToArray());

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    
        private void InitContextItem(ContextItem item, ContextItemData node)
        {
            item.nameText.text = node.name;

            bool hasSubMenu = node.IsTerminal == false;
            item.subContextIcon.gameObject.SetActive(hasSubMenu);

            if (hasSubMenu)
            {
                item.button.onClick.AddListener(()=> CreateSubContext(node, item.subContextTransform));
            }
            else
            {
                item.button.onClick.AddListener(() =>{
                    node.callback?.Invoke();
                    
                    Clear();
                    Hide();
                });
            }
        }

        private void CreateSubContext(ContextItemData node, Transform holder)
        {
            var container = Instantiate(contextContainerPrefab, holder).GetComponent<ContextContainer>();

            PopulateContainer(container, node.children.ToArray());
            OnMenuItemClick?.Invoke(node, container);
        }

        private void PopulateContainer(ContextContainer container, ContextItemData[] data)
        {
            foreach (var node in data)
            {
                container.depthLevel = node.Level;
                var contextItem = Instantiate(contextItemPrefab, container.content).GetComponent<ContextItem>();
                InitContextItem(contextItem, node);
            }
        }
    }
}
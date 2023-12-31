﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Checkers
{
    public abstract class BaseClickComponent : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private MeshRenderer _mesh;
        private ColorType? _color;

        public bool IsHighlighted { get; private set; }

        /// <summary>
        /// Возвращает цветовую сторону игрового объекта
        /// </summary>
        public ColorType GetColor => _color ?? throw new ArgumentNullException("Цвет не установлен!");
        public void SetColor(ColorType color)
        {
            if (_color.HasValue)
            {
                Debug.LogError("Цвет уже установлен.");
                return;
            }

            _color = color;
        }
        
        protected abstract Material GetBaseMaterial();
        protected abstract Material GetMaterialForHighlighted();

        private void SetMaterial(Material material = null)
            => _mesh.sharedMaterial = material ? material : GetBaseMaterial();

        public void SetFocused(bool focus)
        {
            SetMaterial(focus
                ? GetMaterialForHighlighted()
                : GetBaseMaterial());
        }
        
        public void SetHighlighted(bool selected)
        {
            IsHighlighted = selected;
            SetFocused(selected);
        }

        /// <summary>
        /// Возвращает или устанавливает пару игровому объекту
        /// </summary>
        /// <remarks>У клеток пара - фишка, у фишек - клетка</remarks>
        public BaseClickComponent Pair { get; private set; }

        public void SetNewPair(BaseClickComponent pair)
        {
            if (Pair is not null)
            {
                Pair.Pair = null;
            }

            Pair = pair;
        }

        /// <summary>
        /// Событие клика на игровом объекте
        /// </summary>
        public event ClickEventHandler OnClickEventHandler;

        /// <summary>
        /// Событие наведения и сброса наведения на объект
        /// </summary>
        public event FocusEventHandler OnFocusEventHandler;

        //При навадении на объект мышки, вызывается данный метод
        //При наведении на фишку, должна подсвечиваться клетка под ней
        //При наведении на клетку - подсвечиваться сама клетка
        public abstract void OnPointerEnter(PointerEventData eventData);

        //Аналогично методу OnPointerEnter(), но срабатывает когда мышка перестает
        //указывать на объект, соответственно нужно снимать подсветку с клетки
        public abstract void OnPointerExit(PointerEventData eventData);

        //При нажатии мышкой по объекту, вызывается данный метод
        public void OnPointerClick(PointerEventData eventData)
		{
            OnClickEventHandler?.Invoke(this);
        }

        //Этот метод можно вызвать в дочерних классах (если они есть) и тем самым пробросить вызов
        //события из дочернего класса в родительский
        protected void CallBackEvent(CellComponent target, bool isSelect)
        {
            OnFocusEventHandler?.Invoke(target, isSelect);
		}

		protected virtual void Start()
        {
            _mesh = GetComponent<MeshRenderer>();
            SetMaterial();
        }
	}

    public enum ColorType
    {
        White,
        Black
    }

    public delegate void ClickEventHandler(BaseClickComponent component);
    public delegate void FocusEventHandler(CellComponent component, bool isSelect);
}
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Utils;

namespace KursAM2.DragAndDrop
{
    public class ExternalDragDropViewInfo : DragDropViewInfo
    {
        public new static readonly DependencyProperty DraggingRowsProperty;
        public new static readonly DependencyProperty DropTargetRowProperty;
        public new static readonly DependencyProperty DropTargetTypeProperty;
        public new static readonly DependencyProperty FirstDraggingObjectProperty;
        public new static readonly DependencyProperty GroupInfoProperty;

        static ExternalDragDropViewInfo()
        {
            var ownerType = typeof(ExternalDragDropViewInfo);
            DraggingRowsProperty = DependencyPropertyManager.Register("DraggingRows", typeof(IList), ownerType,
                new UIPropertyMetadata(null));
            DropTargetTypeProperty = DependencyPropertyManager.Register("DropTargetType", typeof(DropTargetType),
                ownerType, new UIPropertyMetadata(DropTargetType.None));
            DropTargetRowProperty = DependencyPropertyManager.Register("DropTargetRow", typeof(object), ownerType,
                new UIPropertyMetadata(null));
            GroupInfoProperty = DependencyPropertyManager.Register("GroupInfo", typeof(IList<GroupInfo>), ownerType,
                new UIPropertyMetadata(null));
            FirstDraggingObjectProperty = DependencyPropertyManager.Register("FirstDraggingObject", typeof(object),
                ownerType, new UIPropertyMetadata(null));
        }

        public new IList DraggingRows
        {
            get => (IList)GetValue(DraggingRowsProperty);
            set => SetValue(DraggingRowsProperty, value);
        }

        public new object DropTargetRow
        {
            get => GetValue(DropTargetRowProperty);
            set => SetValue(DropTargetRowProperty, value);
        }

        public new DropTargetType DropTargetType
        {
            get => (DropTargetType)GetValue(DropTargetTypeProperty);
            set => SetValue(DropTargetTypeProperty, value);
        }

        public new object FirstDraggingObject
        {
            get => GetValue(FirstDraggingObjectProperty);
            set => SetValue(FirstDraggingObjectProperty, value);
        }

        public new IList<GroupInfo> GroupInfo
        {
            get => (IList<GroupInfo>)GetValue(GroupInfoProperty);
            set => SetValue(GroupInfoProperty, value);
        }
    }
}

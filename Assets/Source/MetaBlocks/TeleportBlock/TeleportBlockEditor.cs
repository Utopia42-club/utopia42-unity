using System;
using System.Collections.Generic;
using Source.Model;
using Source.Service;
using Source.Service.Auth;
using Source.Ui.LoadingLayer;
using Source.Ui.Snack;
using Source.Ui.Utils;
using UnityEngine.UIElements;

namespace Source.MetaBlocks.TeleportBlock
{
    public class TeleportBlockEditor
    {
        private readonly TeleportPropertiesEditor editorElement;

        public TeleportBlockEditor(Action<TeleportBlockProperties> onSave, int instanceID)
        {
            editorElement = PropertyEditor.INSTANCE.Setup(new TeleportPropertiesEditor(),
                "Teleport Block Properties", () =>
                {
                    onSave(GetValue());
                    PropertyEditor.INSTANCE.Hide();
                }, instanceID);
        }

        public TeleportBlockProperties GetValue()
        {
            return editorElement.GetValue();
        }

        public void SetValue(TeleportBlockProperties value)
        {
            editorElement.SetValue(value);
        }

        public void Show()
        {
            PropertyEditor.INSTANCE.Show();
        }
    }
}
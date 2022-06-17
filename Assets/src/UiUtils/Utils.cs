using UnityEngine;
using UnityEngine.UIElements;

namespace src.UiUtils
{
    public class Utils
    {
        public static void RegisterUiEngagementCallbacksForTextField(TextField textField)
        {
            int? engagementId = null;
            textField.RegisterCallback<FocusInEvent>(evt => { engagementId = GameManager.INSTANCE.EngageUi(); });
            textField.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (engagementId != null)
                    GameManager.INSTANCE.UnEngageUi(engagementId.Value);
            });
        }

        public static void SetPlaceHolderForTextField(TextField textField, string placeHolder)
        {
            textField.RegisterCallback<FocusInEvent>(evt =>
            {
                if (textField.text == placeHolder)
                    textField.SetValueWithoutNotify("");
            });
            textField.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (string.IsNullOrEmpty(textField.text))
                    textField.SetValueWithoutNotify(placeHolder);
            });
        }
    }
}
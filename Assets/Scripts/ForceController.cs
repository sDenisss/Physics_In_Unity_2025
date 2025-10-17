// using UnityEngine;
// using UnityEngine.UI;

// public class ForceController : MonoBehaviour
// {
//     public PushCube pushCube;
//     public Slider forceSlider;
//     public Text sliderValueText;
    
//     void Start()
//     {
//         forceSlider.onValueChanged.AddListener(OnForceChanged);
//     }
    
//     private void OnForceChanged(float value)
//     {
//         pushCube.SetAppliedForce(value);
//         if (sliderValueText != null)
//             sliderValueText.text = value.ToString("F1");
//     }
    
//     public void ApplyImpulse(float force)
//     {
//         pushCube.SetAppliedForce(force);
//         forceSlider.value = force;
//     }
// }
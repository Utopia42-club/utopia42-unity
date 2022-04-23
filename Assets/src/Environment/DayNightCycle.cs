using UnityEngine;

namespace src.Environment
{
    public class DayNightCycle : MonoBehaviour
    {
        private const float MinTime = 0f;
        private const float MaxTime = 1f;

        public float rotationOffset = 0.1f;

        [Range(0.0f, 1.0f)] public float time;
        public float startTime = 0.4f;
        public Vector3 noon; // rotation of the sun at noon

        [Header("Speed")] public float fullDayLength; //seconds
        public AnimationCurve dayNightSpeed;
        private float timeRate; //Speed of changing time

        [Header("Sun")] public Light sun;
        public Gradient sunColor;
        public AnimationCurve sunIntensity;

        [Header("Moon")] public Light moon;
        public Gradient moonColor;
        public AnimationCurve moonIntensity;

        [Header("Other Lighting")] public AnimationCurve lightingIntensityMultiplier;
        public AnimationCurve reflectionsIntensityMultiplier;

        void Start()
        {
            timeRate = 1.0f / fullDayLength;
            time = startTime;
        }

        private float ComputeSpeed()
        {
            if (time < rotationOffset)
                return 10f;
            if (time > rotationOffset + 0.5f)
                return 10f;
            return 1;
        }

        void Update()
        {
            // update time
            // time += timeRate * ComputeSpeed() * Time.deltaTime;

            if (time >= MaxTime)
                time = MinTime;

            // light rotation
            sun.transform.eulerAngles = (time - rotationOffset) * noon * 4.0f;
            moon.transform.eulerAngles = (time - (1 - rotationOffset)) * noon * 4.0f;

            // light intensity
            sun.intensity = sunIntensity.Evaluate(time);
            moon.intensity = moonIntensity.Evaluate(time);

            // change colors
            sun.color = sunColor.Evaluate(time);
            moon.color = moonColor.Evaluate(time);

            // enable/disable sun/moon
            if (sun.intensity == 0 && sun.gameObject.activeInHierarchy)
                sun.gameObject.SetActive(false);
            else if (sun.intensity > 0 && !sun.gameObject.activeInHierarchy)
                sun.gameObject.SetActive(true);

            if (moon.intensity == 0 && moon.gameObject.activeInHierarchy)
                moon.gameObject.SetActive(false);
            else if (moon.intensity > 0 && !moon.gameObject.activeInHierarchy)
                moon.gameObject.SetActive(true);

            // lighting and reflection intensity
            RenderSettings.ambientIntensity = lightingIntensityMultiplier.Evaluate(time);
            RenderSettings.reflectionIntensity = reflectionsIntensityMultiplier.Evaluate(time);
        }
    }
}
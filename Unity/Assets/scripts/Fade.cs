using System;
using Assets.scripts;

namespace CrankcaseAudio {

    class Fade 
    {
        private float fadeTimeMS;
        private float startValue;
        private  float endValue;

        private float currentElapsedTime = 0.0f;
        public Action completionAction;

        public bool isComplete { get; private set; }
        public eCurveType Curve { get; set; }

        public Fade(float fadeTimeMS, float startValue, float endValue, eCurveType curve) {
            this.Curve = curve;
            this.fadeTimeMS = fadeTimeMS;
            this.startValue = startValue;
            this.endValue = endValue;
            isComplete = false;
        }

        public float Update(float timeDeltaMS) {
            currentElapsedTime += timeDeltaMS;
            if(currentElapsedTime > fadeTimeMS) {
                currentElapsedTime = fadeTimeMS;
                isComplete = true;
                completionAction?.Invoke();
                return endValue;
            }

            float percentage = (float)CrankcaseAudio.Curve.Convert((double)(currentElapsedTime / fadeTimeMS), this.Curve);
            
            return (endValue - startValue) * percentage + startValue;
        }


    }

}
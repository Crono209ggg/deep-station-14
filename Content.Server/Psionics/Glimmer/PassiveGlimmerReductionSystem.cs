using Robust.Shared.Timing;
using Robust.Shared.Configuration;
using Content.Shared.CCVar;
using Content.Shared.Psionics.Glimmer;

namespace Content.Server.Psionics.Glimmer
{
    /// <summary>
    /// Handles the passive reduction of glimmer.
    /// </summary>
    public sealed class PassiveGlimmerReductionSystem : EntitySystem
    {
        [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;

        /// List of glimmer values spaced by minute.
        public List<int> GlimmerValues = new();

        public TimeSpan TargetUpdatePeriod = TimeSpan.FromSeconds(6);
        public TimeSpan NextUpdateTime = default!;
        public TimeSpan LastUpdateTime = default!;

        private float _glimmerLinearDecay;
        private bool _enabled;

        public override void Initialize()
        {
            base.Initialize();
            _cfg.OnValueChanged(CCVars.GlimmerLinearDecayPerMinute, UpdatePassiveGlimmer, true);
            _enabled = _cfg.GetCVar(CCVars.GlimmerEnabled);
            _cfg.OnValueChanged(CCVars.GlimmerEnabled, value => _enabled = value, true);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            if (!_enabled)
                return;

            var curTime = _timing.CurTime;
            if (NextUpdateTime > curTime)
                return;

            var glimmerDecay = _glimmerLinearDecay / (60 / TargetUpdatePeriod.Seconds);

            _glimmerSystem.DeltaGlimmerOutput(-glimmerDecay);

            NextUpdateTime = curTime + TargetUpdatePeriod;
            LastUpdateTime = curTime;
        }

        private void UpdatePassiveGlimmer(float value) => _glimmerLinearDecay = value;
    }
}

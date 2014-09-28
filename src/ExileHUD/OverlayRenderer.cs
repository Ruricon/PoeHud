using System;
using System.Collections.Generic;
using ExileHUD.ExileBot;

namespace ExileHUD.ExileHUD
{
	public class OverlayRenderer
	{
		private List<HUDPlugin> hudRenderers;
		private PathOfExile poe;
		public XPHRenderer XphRenderer;
		public PreloadAlert PreloadAlert;
		public MinimapRenderer MinimapRenderer;
		private int modelUpdatePeriod;
		public OverlayRenderer(PathOfExile poe, RenderingContext rc)
		{
			this.poe = poe;
			poe.Area.OnAreaChange += area => modelUpdatePeriod = 6;

			this.MinimapRenderer = new MinimapRenderer();
			this.XphRenderer = new XPHRenderer();
			this.PreloadAlert = new PreloadAlert();
			this.hudRenderers = new List<HUDPlugin>{
				new HealthBarRenderer(),
				new ItemAlerter(),
				this.MinimapRenderer,
				new ItemLevelRenderer(),
				new ItemRollsRenderer(),
				new DangerAlert(),
				this.XphRenderer,
				new ClientHacks(),
				this.PreloadAlert
			};
			if (Settings.GetBool("Window.ShowIngameMenu"))
			{
	#if !DEBUG
				this.hudRenderers.Add(new Menu());
	#endif
			}
			rc.OnRender += this.rc_OnRender;

			this.hudRenderers.ForEach(x => x.Init(poe, this));
		}

		private void rc_OnRender(RenderingContext rc)
		{
			if (!Settings.GetBool("Window.RequireForeground") || this.poe.Window.IsForeground())
			{
				this.modelUpdatePeriod++;
				if (this.modelUpdatePeriod > 6)
				{
					this.poe.Update();
					this.modelUpdatePeriod = 0;
				}
				if (!this.poe.InGame || this.poe.Player == null)
				{
					return;
				}
				foreach (HUDPlugin current in this.hudRenderers)
				{
					current.Render(rc);
				}
			}
		}
		public bool Detach()
		{
			foreach (HUDPlugin current in this.hudRenderers)
			{
				current.OnDisable();
			}
			return false;
		}
	}
}

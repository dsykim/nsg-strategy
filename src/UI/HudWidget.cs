using Godot;

/**
 * Base for every HUD element. Owns the HudContext and the subscription lifecycle:
 * refresh() runs on init and on every StateChanged (which includes turn changes).
 *
 * Subclasses that need more events add them in onSubscribe/onUnsubscribe rather than
 * overriding init, so teardown stays symmetric.
 *
 * Extends Control, so the script can sit on any Control-derived node
 * (PanelContainer, HBoxContainer, MarginContainer...). Exports are optional by convention;
 * the helpers below skip unassigned nodes.
 */
public abstract partial class HudWidget : Control
{
	protected HudContext ctx { get; private set; }

	public void init(HudContext context) {
		teardown();
		ctx = context;
		ctx.executor.StateChanged += onChanged;
		onSubscribe();
		refresh();
	}

	public override void _ExitTree() => teardown();

	private void teardown() {
		if (ctx == null) return;
		ctx.executor.StateChanged -= onChanged;
		onUnsubscribe();
	}

	private void onChanged() => refresh();

	/** Subscribe to any extra events. ctx is set; refresh() runs right after. */
	protected virtual void onSubscribe() { }

	/** Undo everything onSubscribe did. */
	protected virtual void onUnsubscribe() { }

	/** Pull everything from ctx and update the display. Only called after init. */
	protected abstract void refresh();

	/* ==================== Helpers ==================== */

	protected static void setText(Label label, string text) {
		if (label != null) label.Text = text;
	}

	protected static void setBar(Range bar, double value, double max) {
		if (bar == null) return;
		bar.MaxValue = max;
		bar.Value = value;
	}

	protected static string capitalize(string s) =>
			string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1);
}

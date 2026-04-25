namespace UI;

using Godot;

public sealed partial class SegmentBar : Control {
	public readonly record struct Segment(float Value, Color Color);

	private Segment[] Segments = [];

	public void SetSegments(params Segment[] segments) {
		Segments = segments;
		QueueRedraw();
	}

	public override void _Draw() {
		if(Segments.Length == 0) { return; }

		float total = 0f;
		foreach(Segment s in Segments) { total += s.Value; }
		if(total <= 0f) { return; }

		float x = 0f;
		float height = Size.Y;
		float width = Size.X;

		foreach(Segment s in Segments) {
			float segmentWidth = s.Value / total * width;
			DrawRect(new Rect2(x, 0f, segmentWidth, height), s.Color);
			x += segmentWidth;
		}
	}
}

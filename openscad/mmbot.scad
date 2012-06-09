include <mcad/shapes.scad>

module body_frame_left()
{
	translate([-27.5,0,0])
	difference()
	{
		box(55,70,110);
		translate([10,10,0]) box(55,70,90);
	}
}

module body_frame_right()
{
	mirror([1,0,0])
	{
		body_frame_left();
	}
}


render(10)
{
	union()
	{	
		body_frame_left();
		body_frame_right();
	}

}

using Fusion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridDemo {
	public class GridConfigService: GameService {
		
		[Config]
		public GridConfig	Config { get; set; }

		public class GridConfig{
		
			[Category("Grid")]
			public int MaxRadius { get; set; }

			[Category("Grid")]
			public int RadiusOfFirstCircle { get; set; }

			[Category("Grid")]
			public int InitialSize { get; set; }

			[Category( "Points" )]
			public int NumberOfLayers { get; set; }

			[Category( "Points" )]
			public float DistanceBetweenLayers { get; set; }

			[Category( "Points" )]
			public int NumberOfCircles { get; set; }
						
			[Category( "Points" )]
			public float Height { get; set; }
			
			[Category( "Points" )]
			public float Size { get; set; }

			[Category( "Points" )]
			public float Step { get; set; }

			[Category( "Points" )]
			public bool Randomness { get; set; }

			[Category( "Points" )]
			public bool FollowCamera { get; set; }

			public GridConfig(){
				MaxRadius				= 300;
				RadiusOfFirstCircle		= 10;
				InitialSize				= 10;
				NumberOfLayers			= 1;
				DistanceBetweenLayers	= 50;
				NumberOfCircles			= 8;
				Step					= 1;
				Randomness				= false;
				FollowCamera			= false;
				Size					= 0.5f;
				Height					= 50;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public GridConfigService ( Game game ) : base(game)
		{
			Config	=	new GridConfig();
		}
	}

	
}

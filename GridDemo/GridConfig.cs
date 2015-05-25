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

			public GridConfig(){
				MaxRadius = 300;
				RadiusOfFirstCircle = 10;
				InitialSize = 10;
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

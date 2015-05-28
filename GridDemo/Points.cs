using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;
using Fusion.Graphics;
using Fusion.Audio;
using Fusion.Input;
using Fusion.Content;
using Fusion.Development;



namespace GridDemo {
	public class Points: GameService {
		/// <summary>
		/// PhysicsDemo constructor
		/// </summary>
		public Points(Game game)
			: base( game )			
		{
		}

		
		ConstantBuffer constBuffer;
		Ubershader uberShader;
		StateFactory factory;
		Texture2D texture;
		int numberOfPoints = 0;


		struct CBData {
			public Fusion.Mathematics.Matrix Projection;
			public Fusion.Mathematics.Matrix View;
			public Fusion.Mathematics.Matrix World;
			public Vector4 ViewPos;

		}


		enum RenderFlags {
			None,
		}


		struct PointVertex {
			[Vertex("POSITION")]
			public Vector3 Position;
			[Vertex("NORMAL")]
			public Vector3 Normal;
			[Vertex("COLOR")]
			public Vector4 Color;
			[Vertex("TEXCOORD")]
			public Vector2 TexCoord;
		}

		VertexBuffer vb;
		List<PointVertex> list;

		/// <summary>
		/// Adds point at specified position
		/// </summary>
		/// <param name="p"></param>
		public void AddPoint(Vector3 pos, Vector3 normal, Vector4 color, Vector2 texcoord) {
			

			var p = new PointVertex() {
				Position	= pos,
				Normal		= normal, 
				Color		= color,
				TexCoord	= texcoord,
			};

			list.Add( p );
		}

		/// <summary>
		/// Add services :
		/// </summary>
		public override void Initialize ()
		{

			constBuffer = new ConstantBuffer(Game.GraphicsDevice, typeof(CBData));

			vb = new VertexBuffer( Game.GraphicsDevice, typeof( PointVertex ), 128*128 );
			list = new List<PointVertex>();

			//add grid
			Vector3 start = new Vector3( -64, 0, -64 );
			for (int i = 0; i < 128; i++) {
				for (int j = 0; j < 128; j++) {
					Vector3 position = new Vector3( start.X + i, 50, start.Z + j );
					AddPoint( position, Vector3.Up, Color.White.ToVector4(), Vector2.Zero );
				}
			}
			
			
			numberOfPoints = list.Count;
			vb.SetData( list.ToArray(), 0, numberOfPoints );
			//AddPoint( new Vector3( -0.5f, -0.5f, -0.5f ), new Vector3( -1.0f, 0, 0 ), color, texcoord );
			//AddPoint( new Vector3( -0.5f, -0.5f,  0.5f ), new Vector3( -1.0f, 0, 0 ), color, texcoord );
			//AddPoint( new Vector3( -0.5f, 0.5f, 0.5f ),   new Vector3( -1.0f, 0, 0 ), color, texcoord );
			//AddPoint( new Vector3( -0.5f, 0.5f, -0.5f ),  new Vector3( -1.0f, 0, 0 ), color, texcoord );

			//AddPoint( new Vector3( 0.5f, -0.5f, -0.5f ), new Vector3( 1.0f, 0, 0 ), color, texcoord );
			//AddPoint( new Vector3( 0.5f, -0.5f, 0.5f ),  new Vector3( 1.0f, 0, 0 ), color, texcoord );
			//AddPoint( new Vector3( 0.5f, 0.5f, 0.5f ),   new Vector3( 1.0f, 0, 0 ), color, texcoord );
			//AddPoint( new Vector3( 0.5f, 0.5f, -0.5f ),  new Vector3( 1.0f, 0, 0 ), color, texcoord );
			
			//AddPoint( new Vector3( -0.5f, -0.5f, -0.5f ), new Vector3( 0, 0, -1.0f ), color, texcoord );
			//AddPoint( new Vector3( 0.5f, -0.5f, -0.5f ),  new Vector3( 0, 0, -1.0f ), color, texcoord );
			//AddPoint( new Vector3( 0.5f, 0.5f, -0.5f ),   new Vector3( 0, 0, -1.0f ), color, texcoord );
			//AddPoint( new Vector3( -0.5f, 0.5f, -0.5f ),  new Vector3( 0, 0, -1.0f ), color, texcoord );

			//AddPoint( new Vector3( -0.5f, -0.5f, 0.5f ), new Vector3( 0, 0, 1.0f ), color, texcoord );
			//AddPoint( new Vector3( 0.5f, -0.5f, 0.5f ),  new Vector3( 0, 0, 1.0f ), color, texcoord );
			//AddPoint( new Vector3( 0.5f, 0.5f, 0.5f ),   new Vector3( 0, 0, 1.0f ), color, texcoord );
			//AddPoint( new Vector3( -0.5f, 0.5f, 0.5f ),  new Vector3( 0, 0, 1.0f ), color, texcoord );

			//AddPoint( new Vector3( 0.5f, 0.5f, -0.5f ),  new Vector3( 0, 1.0f, 0 ), color, texcoord );
			//AddPoint( new Vector3( 0.5f, 0.5f, 0.5f ),   new Vector3( 0, 1.0f, 0 ), color, texcoord );
			//AddPoint( new Vector3( -0.5f, 0.5f, 0.5f ),  new Vector3( 0, 1.0f, 0 ), color, texcoord );
			//AddPoint( new Vector3( -0.5f, 0.5f, -0.5f ), new Vector3( 0, 1.0f, 0 ), color, texcoord );

			//AddPoint( new Vector3( 0.5f, -0.5f, -0.5f ),  new Vector3( 0, -1.0f, 0 ), color, texcoord );
			//AddPoint( new Vector3( 0.5f, -0.5f, 0.5f ),   new Vector3( 0, -1.0f, 0 ), color, texcoord );
			//AddPoint( new Vector3( -0.5f, -0.5f, 0.5f ),  new Vector3( 0, -1.0f, 0 ), color, texcoord );
			//AddPoint( new Vector3( -0.5f, -0.5f, -0.5f ), new Vector3( 0, -1.0f, 0 ), color, texcoord );

			base.Initialize();

			Game.Reloading += Game_Reloading;
			Game_Reloading( this, EventArgs.Empty );

		}



		void Game_Reloading(object sender, EventArgs e) {
			uberShader = Game.Content.Load<Ubershader>("render");
			factory = new StateFactory( uberShader, typeof( RenderFlags ), (ps, i) => EnumAction( ps, (RenderFlags) i ) );
			texture = Game.Content.Load<Texture2D>("tex");
		}

		void EnumAction(PipelineState ps, RenderFlags flag) {
			ps.Primitive = Primitive.PointList;
			ps.VertexInputElements = VertexInputElement.FromStructure<PointVertex>();
			ps.DepthStencilState = DepthStencilState.Readonly;

		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose (bool disposing)
		{
			if ( disposing ) {
				SafeDispose(ref constBuffer);
				SafeDispose(ref vb);
			}

			base.Dispose(disposing);
		}





		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update (GameTime gameTime)
		{
			base.Update(gameTime);
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		public override void Draw(GameTime gameTime, Fusion.Graphics.StereoEye stereoEye)
		{
			CBData cbData = new CBData();

			var cam = Game.GetService<Camera>();

			cbData.Projection = cam.GetProjectionMatrix(stereoEye);
			cbData.View = cam.GetViewMatrix(stereoEye);
			cbData.World = Matrix.Identity;
			cbData.ViewPos = new Vector4(cam.GetCameraMatrix(stereoEye).TranslationVector, 1);


			constBuffer.SetData(cbData);
			
			Game.GraphicsDevice.PipelineState = factory[0];

			Game.GraphicsDevice.PixelShaderConstants[0] = constBuffer;
			Game.GraphicsDevice.VertexShaderConstants[0] = constBuffer;
			Game.GraphicsDevice.PixelShaderSamplers[0] = SamplerState.LinearWrap;
			Game.GraphicsDevice.PixelShaderResources[0] = texture;

			// setup data and draw points
			Game.GraphicsDevice.SetupVertexInput( vb, null );
			Game.GraphicsDevice.Draw(numberOfPoints, 0);

						
			base.Draw(gameTime, stereoEye);
		}
	}
}

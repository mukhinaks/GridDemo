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

		//parameters of grid
		int		numberOfLayers;
		float	distanceBetweenLayers;
		float	step;
		bool	randomness;
		bool	followCamera;
		int		numberOfCircles;


		struct CBData {
			public Fusion.Mathematics.Matrix Projection;
			public Fusion.Mathematics.Matrix View;
			public Fusion.Mathematics.Matrix World;
			public Vector4 CameraPos;

		}


		enum RenderFlags {
			None,
			RELATIVE	= 0x1,
			FIXED		= 0x2,
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

		//create a standard rectangular ring
		private void CreateRing(int dimension, Vector3 leftCorner, float step, Color color, bool offset) {

			for (int i = 0; i < dimension; i++) {
				AddPoint( leftCorner + Vector3.UnitZ * step * i, Vector3.Up, color.ToVector4(), Vector2.Zero );
				AddPoint( leftCorner + (Vector3.UnitZ * step * i + Vector3.UnitX * (dimension - 1) * step), Vector3.Up, color.ToVector4(), Vector2.Zero );
			}
			for (int i = 1; i < (dimension - 1); i++) {
				AddPoint( leftCorner + Vector3.UnitX * step * i, Vector3.Up, color.ToVector4(), Vector2.Zero );
				AddPoint( leftCorner + (Vector3.UnitX * i + Vector3.UnitZ * (dimension - 1) ) * step, Vector3.Up, color.ToVector4(), Vector2.Zero );
			}
		}

		//create a last ring
		private void CreateLastRing(int dimension, Vector3 leftCorner, float step, Color color, bool offset) {
			for (int i = 1; i < dimension; i+=2) {
				AddPoint( leftCorner + Vector3.UnitZ * step * i, Vector3.Up, color.ToVector4(), Vector2.Zero );
				AddPoint( leftCorner + (Vector3.UnitZ * step * i + Vector3.UnitX * (dimension - 1) * step), Vector3.Up, color.ToVector4(), Vector2.Zero );
			}
			for (int i = 1; i < (dimension - 1); i+=2) {
				AddPoint( leftCorner + Vector3.UnitX * step * i, Vector3.Up, color.ToVector4(), Vector2.Zero );
				AddPoint( leftCorner + (Vector3.UnitX * i + Vector3.UnitZ * (dimension - 1)) * step, Vector3.Up, color.ToVector4(), Vector2.Zero );
			}
		}

		/// <summary>
		/// Add services :
		/// </summary>
		public override void Initialize()
		{

			var gc = Game.GetService<GridConfigService>();
			distanceBetweenLayers	= gc.Config.DistanceBetweenLayers;
			numberOfLayers			= gc.Config.NumberOfLayers;
			numberOfCircles			= gc.Config.NumberOfCircles;
			randomness				= gc.Config.Randomness;
			followCamera			= gc.Config.FollowCamera;
			step					= gc.Config.Step;

			constBuffer = new ConstantBuffer(Game.GraphicsDevice, typeof(CBData));

			vb = new VertexBuffer( Game.GraphicsDevice, typeof( PointVertex ), 128*128 );
			list = new List<PointVertex>();

			//grid
			Vector3 center = new Vector3( 0, 10, 0 );
			
			for ( int i = 0; i < numberOfLayers; i++ ) {
				Vector3 start = center + Vector3.UnitY * distanceBetweenLayers * i;
				step = gc.Config.Step;

				//add inside square
				AddPoint( start, Vector3.Up, Color.White.ToVector4(), Vector2.Zero );
				start -= Vector3.UnitX + Vector3.UnitZ;
				CreateRing( 3, start, step, Color.White, randomness );
				start -= Vector3.UnitX + Vector3.UnitZ;

				//create circles
				int currentRing = 1;
				while ( currentRing <= numberOfCircles ) {
					CreateRing( 5, start, step, Color.Red, randomness );
					start -= ( Vector3.UnitX + Vector3.UnitZ ) * step;
					CreateRing( 7, start, step, Color.Red, randomness );
					start -= ( Vector3.UnitX + Vector3.UnitZ ) * step;
					CreateLastRing( 9, start, step, Color.LightCyan, randomness );
					step = step * 2;
					currentRing++;
				}

				numberOfPoints = list.Count;
				Log.Message( "{0}", numberOfPoints );
				Log.Message( "{0}", start );
			}
			
			//fill vertex buffer
			numberOfPoints = list.Count;
			Log.Message( "{0}", numberOfPoints );
			vb.SetData( list.ToArray(), 0, numberOfPoints );
			
			base.Initialize();

			Game.Reloading += Game_Reloading;
			Game_Reloading( this, EventArgs.Empty );

		}



		void Game_Reloading(object sender, EventArgs e) {
			uberShader = Game.Content.Load<Ubershader>("points");
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
			cbData.CameraPos = new Vector4(cam.FreeCamPosition, 0);
//			cbData.ViewPos = new Vector4( cam.GetCameraMatrix( stereoEye ).TranslationVector, 1 );


			constBuffer.SetData(cbData);

			if ( followCamera ) {
				Game.GraphicsDevice.PipelineState = factory[(int) RenderFlags.RELATIVE];
			}
			else {
				Game.GraphicsDevice.PipelineState = factory[(int) RenderFlags.FIXED];
			}

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

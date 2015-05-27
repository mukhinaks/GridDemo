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
	public class GridDemo: Game {
		/// <summary>
		/// GridDemo constructor
		/// </summary>
		public GridDemo ()
			: base()
		{
			
			//	enable object tracking :
			Parameters.TrackObjects		=	false;
			Parameters.VSyncInterval	=	0;
			Parameters.MsaaLevel		=	4;

			//Parameters.StereoMode	=	StereoMode.NV3DVision;

			//	add services :
			AddService( new SpriteBatch( this ), false, false, 0, 0 );
			AddService( new DebugStrings( this ), true, true, 9999, 9999 );
			AddService( new DebugRender( this ), true, true, 9998, 9998 );
			AddService( new Camera( this ), true, false, 1, 1 );

			AddService( new ParticleSystemGS( this ), true, true, 500, 500 );
			AddService( new GridConfigService( this ), false, false, 1000, 1000 );
			
			//	load configuration :
			LoadConfiguration();

			//	Force to enable free camera.
			GetService<Camera>().Config.FreeCamEnabled	=	true;

			//	make configuration saved on exit
			Exiting += FusionGame_Exiting;
			InputDevice.KeyDown += InputDevice_KeyDown;
		}



		class Material {
			public Texture2D	Texture;
		}

		class Context {
			public Matrix	View;
			public Matrix	Projection;
			public Vector4	ViewPosition;
		}



		struct CBData {
			public Matrix	Projection;
			public Matrix	View;
			public Matrix	World;
			public Vector4	ViewPos;
		}


		enum RenderFlags {
			None,
		}


		Scene			scene;

		VertexBuffer[]	vertexBuffers;
		IndexBuffer[]	indexBuffers;
		Texture2D[]		textures;
		Matrix[]		worldMatricies;

		ConstantBuffer	constBuffer;
		Ubershader		uberShader;
		StateFactory	factory;
		CBData			constData;


		//visibility radius
		int radius;

		//minimum radius
		int radiusMin;
		//
		int size;

		/// <summary>
		/// Add services :
		/// </summary>
		protected override void Initialize ()
		{
			base.Initialize();

			LoadContent();
			Reloading += (s,e) => LoadContent();
			GetService<Camera>().FreeCamPosition = Vector3.Up * 10;
			constBuffer	=	new ConstantBuffer( GraphicsDevice, typeof(CBData) );
		}



		/// <summary>
		/// Load content
		/// </summary>
		public void LoadContent ()
		{
			SafeDispose( ref factory );
			SafeDispose( ref vertexBuffers );
			SafeDispose( ref indexBuffers );

			uberShader	=	Content.Load<Ubershader>("render");

			factory		=	new StateFactory( 
								uberShader, 
								typeof(RenderFlags), 
								Primitive.TriangleList, 
								VertexColorTextureNormal.Elements,
								BlendState.Opaque,
								RasterizerState.CullCW,
								DepthStencilState.Default 
							);

			scene		=	Content.Load<Scene>(@"Scenes\testScene");


			vertexBuffers	=	scene.Meshes
							.Select( m => VertexBuffer.Create( GraphicsDevice, m.Vertices.Select( v => VertexColorTextureNormal.Convert(v) ).ToArray() ) )
							.ToArray();

			indexBuffers	=	scene.Meshes
							.Select( m => IndexBuffer.Create( GraphicsDevice, m.GetIndices() ) )
							.ToArray();

			textures		=	scene.Materials
							.Select( mtrl => Content.Load<Texture2D>( mtrl.TexturePath ) )
							.ToArray();

			worldMatricies	=	new Matrix[ scene.Nodes.Count ];
			scene.CopyAbsoluteTransformsTo( worldMatricies );

			radius		= GetService<GridConfigService>().Config.MaxRadius;
			radiusMin	= GetService<GridConfigService>().Config.RadiusOfFirstCircle;
			size		= GetService<GridConfigService>().Config.InitialSize;

			//compute grid
			//int r = radiusMin;
			//List<int> list = new List<int>();
			//while (r <= radius){
			//	list.Add(r);
			//	r = 2 * r;
			//	Log.Message("{0}", r );
			//}
			//list.Add(radius);

			//add particles
			var ps = GetService<ParticleSystemGS>();
			//List<Vector3> pos = new List<Vector3>();
			//pos.Add(new Vector3(0, 8, 4));
			//pos.Add(new Vector3(0, 8, 10));
			//pos.Add(new Vector3(0, 8, 30));
			var camPos = GetService<Camera>().FreeCamPosition;
			Vector3 camXZ = new Vector3 (camPos.X, 50, camPos.Z);
						
			//for (int i = 0; i < 1000; i++) {

			//	Vector3 position = rand.NextVector3( new Vector3( -radius, 50, -radius), 
			//											new Vector3( radius, 50, radius) );
			//	var s = size;
			
			//	ps.AddParticle( position, Vector2.Zero, 9999, s, s);
			//			//Log.Message("{0}  {1}", s, position );

			//}
			
			//add grid
			//Vector3 start = Vector3.Zero;
			Vector3 start = new Vector3(-64, 0, -64);
			for (int i = 0; i < 128; i++) {
				for (int j = 0; j < 128; j++){
				Vector3 position = new Vector3( start.X + i, 10, start.Z + j);
				var s = size;
			
				ps.AddParticle( position, Vector2.Zero, 9999, s, s);
						//Log.Message("{0}  {1}", s, position );
				}
			}
			
			Log.Message("{0}", scene.Nodes.Count( n => n.MeshIndex >= 0 ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref constBuffer );
				SafeDispose( ref factory );
				SafeDispose( ref vertexBuffers );
				SafeDispose( ref indexBuffers );
			}

			base.Dispose( disposing );
		}



		/// <summary>
		/// Handle keys for each demo
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDevice_KeyDown ( object sender, Fusion.Input.InputDevice.KeyEventArgs e )
		{
			if (e.Key == Keys.F1) {
				DevCon.Show(this);
			}

			if (e.Key == Keys.F2) {
				Parameters.ToggleVSync();
			}

			if (e.Key == Keys.F5) {
				Reload();
			}

			if (e.Key == Keys.F12) {
				GraphicsDevice.Screenshot();
			}

			if (e.Key == Keys.Escape) {
				Exit();
			}
		}



		/// <summary>
		/// Save configuration on exit.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void FusionGame_Exiting ( object sender, EventArgs e )
		{
			SaveConfiguration();
		}

		//Vector2 lastPoint;
		//Vector2	lastVel;
		Random	rand = new Random();

		//float Gauss ( float mean, float stdDev )
		//{
		//	//Random rand = new Random(); //reuse this if you are generating many
		//	double u1 = rand.NextDouble(); //these are uniform(0,1) random doubles
		//	double u2 = rand.NextDouble();
		//	double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
		//				 Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
		//	double randNormal =
		//				 mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)

		//	return (float)randNormal;
		//}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update ( GameTime gameTime )
		{
			var ds	=	GetService<DebugStrings>();

			GameTime.AveragingFrameCount = 60;

			ds.Add( Color.Orange, "FPS {0}", gameTime.AverageFrameRate );
			ds.Add( Color.Orange, "FT  {0}", gameTime.AverageFrameTime );
			ds.Add( "F1   - show developer console" );
			ds.Add( "F2   - toggle vsync" );
			ds.Add( "F5   - build content and reload textures" );
			ds.Add( "F12  - make screenshot" );
			ds.Add( "ESC  - exit" );

			var cam	=	GetService<Camera>();
			var dr	=	GetService<DebugRender>();
			dr.View			=	cam.GetViewMatrix( StereoEye.Mono );
			dr.Projection	=	cam.GetProjectionMatrix( StereoEye.Mono );

			
			//var ps = GetService<ParticleSystemGS>();

			//var vp = GraphicsDevice.DisplayBounds;

			//Vector2 target = InputDevice.MousePosition;
			//var vel = (lastPoint - target);

			//if (InputDevice.IsKeyDown(Keys.LeftButton)) {

			//	float len = (lastPoint - target).Length() + 0.001f;

			//	for ( float t=0; t<=len; t+=0.15f) {
			//		ps.AddParticle( Vector2.Lerp( lastPoint, target, t/len ), Vector2.Lerp( lastVel,vel,t/len) * Gauss(10,1), Gauss(5,3), 3, Gauss(50,50) );
			//	}
			//}	
				
			//lastPoint = InputDevice.MousePosition;
			//lastVel = vel;

			//int r = radiusMin;
			//while (r <= radius){
			//	dr.DrawRing(new Vector3(cam.FreeCamPosition.X, 30, cam.FreeCamPosition.Z), r, Color.Orange);
			//	r = 2 * r;
			//}
			//dr.DrawRing(new Vector3(cam.FreeCamPosition.X, 30, cam.FreeCamPosition.Z), radius, Color.Orange);
			dr.DrawGrid(10);

			base.Update( gameTime );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			var cam	=	GetService<Camera>();
			

			GraphicsDevice.ClearBackbuffer( Color.CornflowerBlue, 1, 0 );

			constData.View			=	cam.GetViewMatrix( stereoEye );
			constData.Projection	=	cam.GetProjectionMatrix( stereoEye );
			constData.ViewPos		=	cam.GetCameraPosition4( stereoEye );
			constData.World			=	Matrix.Identity;

			for (int j = 0; j<1; j++) {

				GraphicsDevice.PipelineState			=	factory[0];
				GraphicsDevice.PixelShaderSamplers[0]	=	SamplerState.AnisotropicWrap;
				GraphicsDevice.PixelShaderConstants[0]	=	constBuffer;
				GraphicsDevice.VertexShaderConstants[0]	=	constBuffer;


				for (int i=0; i<scene.Nodes.Count; i++) {

					int meshId	=	scene.Nodes[i].MeshIndex;

					if (meshId<0) {
						continue;
					}

					constData.World	=	worldMatricies[ i ];
					constBuffer.SetData( constData );

					GraphicsDevice.SetupVertexInput( vertexBuffers[meshId], indexBuffers[meshId] );

					foreach ( var subset in scene.Meshes[meshId].Subsets ) {
						GraphicsDevice.PixelShaderResources[0]	=	textures[ subset.MaterialIndex ];
						GraphicsDevice.DrawIndexed( subset.PrimitiveCount * 3, subset.StartPrimitive * 3, 0 );
					}
				}
			}

			base.Draw( gameTime, stereoEye );
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.IO;

namespace 光照立方体
{
    public partial class Form1 : Form
    {
        private Device device = null;
        bool pause = false;
        VertexBuffer vertexBuffer = null;
        Material mtrl;
        Mesh mesh = null;
        float Angle = 0, ViewZ = -6.0f;

        //额外对象
        private List<Material[]> m_meshMaterials = new List<Material[]>(); //定义网格材质对象
        private List<Texture[]> m_meshTextures = new List<Texture[]>(); // 定义网格贴图对象
        private List<Mesh> m_meshLst = new List<Mesh>();


        public Form1()
        {
            InitializeComponent();
        }

        public bool InitializeGraphics()
        {
            try
            {
                PresentParameters presentParams = new PresentParameters();
                presentParams.Windowed = true;				//不是全屏显示，在一个窗口显示
                presentParams.SwapEffect = SwapEffect.Discard;		 //后备缓存交换的方式
                presentParams.EnableAutoDepthStencil = true;			 //允许使用自动深度模板测试
                //深度缓冲区单元为16位二进制数
                presentParams.AutoDepthStencilFormat = DepthFormat.D16;
                device = new Device(0, DeviceType.Hardware, this, 	 //建立设备类对象
          CreateFlags.SoftwareVertexProcessing, presentParams);
                
               
                //设置设备重置事件(device.DeviceReset)事件函数为this.OnResetDevice
                device.DeviceReset += new System.EventHandler(this.OnResetDevice);
                this.OnCreateDevice(device, null);//自定义方法，初始化Device的工作放到这个方法中
                this.OnResetDevice(device, null);//调用设备重置事件(device.DeviceReset)事件函数
            }		//设备重置事件函数要设置Device参数，初始函数中必须调用该函数
            catch (DirectXException)
            {
                return false;
            }
            return true;
        }
        //添加网格
        public long AddMesh(string filePath)
        {
            if (device == null)
                return 0;

            if (File.Exists(filePath) == false)
                return 0;
            //加载顶点集合
            ExtendedMaterial[] materials = null;
            Mesh meshObj = Mesh.FromFile(filePath, MeshFlags.SystemMemory, device, out materials);
            if (meshObj == null)
                return 0;

            //加载纹理和材质
            Texture[] meshTextures = new Texture[materials.Length];
            Material[] meshMaterials = new Material[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                meshMaterials[i] = materials[i].Material3D;
                meshMaterials[i].Ambient = meshMaterials[i].Diffuse;
                // 创建贴图
                if (materials[i].TextureFilename != null)
                    meshTextures[i] = TextureLoader.FromFile(device,
                        filePath.Substring(0, filePath.LastIndexOf('\\')) +
                    "\\" + materials[i].TextureFilename);
                else
                    meshTextures[i] = null;
            }

            //加入缓冲
            m_meshMaterials.Add(meshMaterials);
            m_meshTextures.Add(meshTextures);
            m_meshLst.Add(meshObj);
            return m_meshLst.Count;
        }

        public void OnCreateDevice(object sender, EventArgs e)
        {
            Device dev = (Device)sender;		//注意阴影部分，正方形有6个顶点
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionNormal), 6,
            dev, 0, CustomVertex.PositionNormal.Format, Pool.Default);
            vertexBuffer.Created += new System.EventHandler(this.OnCreateVertexBuffer);
            this.OnCreateVertexBuffer(vertexBuffer, null);
            mtrl = new Material();
            mtrl.Diffuse = System.Drawing.Color.Yellow;		//物体的颜色
            mtrl.Ambient = System.Drawing.Color.Red;			//反射环境光的颜色 
            //mesh = Mesh.FromFile(@"..\..\..\Dwarf.x", MeshFlags.SystemMemory, device);
            AddMesh(@"D:\\Microsoft DirectX SDK (June 2010)\\Samples\\Media\\Dwarf\\Dwarf.x");
        }

        public void OnResetDevice(object sender, EventArgs e)
        {
            Device dev = (Device)sender;
            dev.RenderState.CullMode = Cull.CounterClockwise;		//背面剔除
            device.RenderState.ZBufferEnable = true;				//打开Z缓冲
            device.RenderState.Lighting = true;					//打开灯光
            mtrl = new Material();
            mtrl.Diffuse = System.Drawing.Color.Yellow;			//物体的颜色
            mtrl.Ambient = System.Drawing.Color.Red;				//反射环境光的颜色
            SetupLights();
            Render();
        }

        public void Render()		//渲染方法，本方法没有任何渲染代码，可认为是渲染方法的框架
        {
            if (device == null) //如果未建立设备对象，退出
                return;
            if (pause)
                return;
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.WhiteSmoke, 1.0f, 0);
            device.BeginScene();		//开始渲染
             SetupMatrices();
//             device.SetStreamSource(0, vertexBuffer, 0);
//             device.VertexFormat = CustomVertex.PositionNormal.Format;
//             device.Transform.World = Matrix.Translation(0, 0, -1);
//             //以下和6.6节例子渲染方法Render中内容相同
//             device.Transform.World = Matrix.Translation(0, 0, -1);//沿Z轴向观察者方向移动1个单位
//             device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);    //绘制正前面
//             //旋转180度是为了从外侧看，按顺时针方向绘制三角形，因背面剔除打开，内侧不被看到
//             device.Transform.World = Matrix.RotationY((float)Math.PI) * Matrix.Translation(0, 0, 1);
//             device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);    //绘制正后面
//             device.Transform.World =
//             Matrix.RotationY(-(float)Math.PI / 2) * Matrix.Translation(1, 0, 0);
//             device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);		//绘制右侧面
//             device.Transform.World =
//         Matrix.RotationY((float)Math.PI / 2) * Matrix.Translation(-1, 0, 0);
//             device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);		//绘制左侧面
//             device.Transform.World =
//         Matrix.RotationX((float)Math.PI / 2) * Matrix.Translation(0, 1, 0);
//             device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);		//绘制下面
//             device.Transform.World =
//         Matrix.RotationX(-(float)Math.PI / 2) * Matrix.Translation(0, -1, 0);
//             device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);   	//绘制上面


            //渲染Mesh
            for (int i = 0; i < m_meshLst.Count; i++)
            {
                for (int j = 0; j < m_meshMaterials[i].Length; j++)
                {
                    //                         m_device.Transform.World = Matrix.Scaling(0.2f, 0.2f, 0.2f) * 
                    //                             Matrix.RotationX((float)Math.PI / 2) * 
                    //                             Matrix.Translation(300, 100, 200);

                    //设置网格子集的材质和贴图
                    device.Material = m_meshMaterials[i][j];
                    device.SetTexture(0, m_meshTextures[i][j]);
                    //绘制网格子集
                    m_meshLst[i].DrawSubset(j);
                }
            }



            device.EndScene();										//渲染结束
            device.Present();	//更新显示区域，把后备缓存的D图形送到图形卡的显存中显示

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            this.Render();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            pause = ((this.WindowState == FormWindowState.Minimized) || !this.Visible);
        }

        public void OnCreateVertexBuffer(object sender, EventArgs e)
        {
            CustomVertex.PositionNormal[] verts =
                            (CustomVertex.PositionNormal[])vertexBuffer.Lock(0, 0);
            verts[0].Position = new Vector3(-1.0f, -1.0f, 0.0f); //顶点0位置，注意为Vector3
            verts[0].Normal = new Vector3(0, 0, -1);    	  			  //顶点0法线
            verts[1].Position = new Vector3(1.0f, 1.0f, 0.0f);	  	  //顶点1位置
            verts[1].Normal = new Vector3(0, 0, -1);					  //顶点1法线
            verts[2].Position = new Vector3(1.0f, -1.0f, 0.0f);	      //顶点2位置
            verts[2].Normal = new Vector3(0, 0, -1);
            verts[3].Position = new Vector3(-1.0f, -1.0f, 0.0f);	  //顶点3位置
            verts[3].Normal = new Vector3(0, 0, -1);    	  			  //顶点3法线
            verts[4].Position = new Vector3(-1.0f, 1.0f, 0.0f);	  	  //顶点4位置
            verts[4].Normal = new Vector3(0, 0, -1);
            verts[5].Position = new Vector3(1.0f, 1.0f, 0.0f);	      //顶点5位置
            verts[5].Normal = new Vector3(0, 0, -1);
            vertexBuffer.Unlock();
        }

        private void SetupMatrices()		//注意世界变换和观察变换参数可能要改变
        {
            device.Transform.World = Matrix.RotationY(0);	//世界变换矩阵
            Vector3 v1 = new Vector3(0.0f, 0.0f, -5.0f);		//下句使v1点分别沿Y轴和X轴旋转
            v1.TransformCoordinate(Matrix.RotationYawPitchRoll(Angle, ViewZ, 0));
//             device.Transform.View = Matrix.LookAtLH(v1, new Vector3(0.0f, 0.0f, 0.0f),
//             new Vector3(0.0f, 1.0f, 0.0f));	//观察变换矩阵
            device.Transform.View = Matrix.LookAtLH(new Vector3(0.0f, 0.0f,-5.0f), 
                new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 1.0f, 0.0f));	//观察变换矩阵
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4,
                            (float)this.Width/(float)this.Height, 1.0f, 100.0f);			//投影变换矩阵
        }//需要实时计算(float)this.Width/(float)this.Height

        private void SetupLights()
        {
            device.Material = mtrl;
            device.Lights[0].Type = LightType.Directional;
            device.Lights[0].Diffuse = System.Drawing.Color.White;	//光的颜色为白色
            device.Lights[0].Direction = new Vector3(0, -2, 4);//灯光方向从观察者上方指向屏幕下方
            device.Lights[0].Update();			//更新灯光设置，创建第一盏灯光
            device.Lights[0].Enabled = true;		//使设置有效，下句设置环境光为白色
            device.RenderState.Ambient = System.Drawing.Color.FromArgb(0x808080);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)				//e.KeyCode是键盘每个键的编号
            {
                case Keys.Left:			//Keys.Left是左箭头键编号，三角形沿Y轴左转
                    Angle += 0.1F;
                    break;
                case Keys.Right:			//三角形沿Y轴右转
                    Angle -= 0.1F;
                    break;
                case Keys.Down:			//三角形离观察者越来越远
                    ViewZ += 0.1F;
                    break;
                case Keys.Up:				//三角形离观察者越来越近
                    ViewZ -= 0.1F;
                    break;
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeGraphics();
            this.Show();
            Render();
        }
    }
}

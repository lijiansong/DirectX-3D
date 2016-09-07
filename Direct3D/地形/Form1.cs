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

namespace 地形
{
    public partial class Form1 : Form
    {
        private Device device = null;
        bool pause = false;
        Mesh mesh = null;
        Material[] meshMaterials1;
        Texture[] meshTextures;


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

        public void OnCreateDevice(object sender, EventArgs e)
        {
            Device dev = (Device)sender;
            ExtendedMaterial[] materials = null;
            mesh = Mesh.FromFile(@"..\..\..\seafloor.x",
         MeshFlags.SystemMemory, device, out materials);
            if (meshTextures == null)			//如果还未设置纹理，为D图形增加纹理和材质
            {
                meshTextures = new Texture[materials.Length];				//纹理数组
                meshMaterials1 = new Material[materials.Length];	        //材质数组
                for (int i = 0; i < materials.Length; i++)					//读入纹理和材质
                {
                    meshMaterials1[i] = materials[i].Material3D;
                    meshMaterials1[i].Ambient = meshMaterials1[i].Diffuse;
                    meshTextures[i] = TextureLoader.FromFile(device, @"..\..\..\" +
                                               materials[i].TextureFilename);
                }
            }			//下条语句克隆mesh对象，使其包含位置、法线和纹理坐标
            Mesh mesh1 = mesh.Clone(mesh.Options.Value, VertexFormats.Position | VertexFormats.Normal |
                             VertexFormats.Texture0 | VertexFormats.Texture1, mesh.Device);
            using (VertexBuffer vb = mesh1.VertexBuffer)	//得到mesh1记录顶点的缓冲区引用
            {
                CustomVertex.PositionNormalTextured[] verts = (CustomVertex.PositionNormalTextured[])
          vb.Lock(0, typeof(CustomVertex.PositionNormalTextured),
          LockFlags.None, mesh1.NumberVertices);
                try
                {
                    for (int i = 0; i < verts.Length; i++)
                    {
                        verts[i].Y = HeightField(verts[i].X, verts[i].Z);
                    }
                    mesh = mesh1;
                }
                finally
                {
                    vb.Unlock();
                }
            }


        }

        public void OnResetDevice(object sender, EventArgs e)
        {
            Device dev = (Device)sender;
            dev.RenderState.ZBufferEnable = true;		 //允许使用深度缓冲，意义见.12.5节
            dev.RenderState.Ambient = Color.FromArgb(255, 200, 200, 200);	     //环境光为黑色 
            dev.RenderState.Lighting = true;
            dev.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
            dev.TextureState[0].ColorOperation = TextureOperation.SelectArg1;
            dev.SamplerState[0].MinFilter = TextureFilter.Linear;
            dev.SamplerState[0].MagFilter = TextureFilter.Linear;
            SetupMatrices();


        }

        public void Render()		//渲染方法，本方法没有任何渲染代码，可认为是渲染方法的框架
        {
            if (device == null) //如果未建立设备对象，退出
                return;
            int iTime = Environment.TickCount % 100000;
            float Angle = iTime * (2.0f * (float)Math.PI) / 100000.0f;
            device.Transform.World = Matrix.RotationY(Angle);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, System.Drawing.Color.Blue, 1.0f, 0);
            device.BeginScene();//开始渲染
            for (int i = 0; i < meshMaterials1.Length; i++)		//Mesh中可能有多个D图形，逐一显示
            {
                device.Material = meshMaterials1[i];		//设定D图形的材质
                device.SetTexture(0, meshTextures[i]);		//设定D图形的纹理
                mesh.DrawSubset(i);						//显示该D图形
            }
            device.EndScene();//渲染结束
            device.Present();//更新显示区域，把后备缓存的D图形送到显卡的显存中显示

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            this.Render();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            pause = ((this.WindowState == FormWindowState.Minimized) || !this.Visible);
        }

        private void SetupMatrices()
        {
            device.Transform.World = Matrix.Identity;		//世界坐标
            device.Transform.View = Matrix.LookAtLH(new Vector3(0.0f, 30.0f, -100.0f), //观察变换
            new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, 1.0f, 1.0f, 200.0f);		//投影变换
        }

        float HeightField(float x, float z)		//参数为图形某点的X、Z轴坐标
        {
            float y = 0.0f;
            y += (float)(10.0f * Math.Cos(0.051f * x + 0.0f) * Math.Sin(0.055f * x + 0.0f));
            y += (float)(10.0f * Math.Cos(0.053f * z + 0.0f) * Math.Sin(0.057f * z + 0.0f));
            y += (float)(2.0f * Math.Cos(0.101f * x + 0.0f) * Math.Sin(0.105f * x + 0.0f));
            y += (float)(2.0f * Math.Cos(0.103f * z + 0.0f) * Math.Sin(0.107f * z + 0.0f));
            y += (float)(2.0f * Math.Cos(0.251f * x + 0.0f) * Math.Sin(0.255f * x + 0.0f));
            y += (float)(2.0f * Math.Cos(0.253f * z + 0.0f) * Math.Sin(0.257f * z + 0.0f));
            return y;		//返回修改后的Y轴方向上的坐标
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeGraphics();
            this.Show();
            Render();
        }
    }
}

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

namespace Mesh读取x文件
{
    public partial class Form1 : Form
    {
        private Device device = null;
        bool pause = false;
        Mesh mesh = null;
        Material meshMaterials;
        Texture[] meshTextures;
        Microsoft.DirectX.Direct3D.Material[] meshMaterials1;
        float Angle = 0, ViewZ = -5.0f;

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
            meshMaterials = new Material();
            meshMaterials.Ambient = System.Drawing.Color.White;		//材质如何反射环境光
            meshMaterials.Diffuse = System.Drawing.Color.White;		//材质如何反射灯光
            ExtendedMaterial[] materials = null;
            //下句从tiger.x文件中读入3D图形(立体老虎)
            mesh = Mesh.FromFile(@"..\..\..\tiger.x", MeshFlags.SystemMemory, device, out materials);
            if (meshTextures == null)//如果还未设置纹理，为3D图形增加纹理和材质
            {
                meshTextures = new Texture[materials.Length];//纹理数组
                meshMaterials1 = new Microsoft.DirectX.Direct3D.Material[materials.Length];//材质数组
                for (int i = 0; i < materials.Length; i++)//读入纹理和材质
                {
                    meshMaterials1[i] = materials[i].Material3D;
                    meshMaterials1[i].Ambient = meshMaterials1[i].Diffuse;
                    meshTextures[i] = TextureLoader.FromFile(device,
                                       @"..\..\..\" + materials[i].TextureFilename);
                }
            }

        }

        public void OnResetDevice(object sender, EventArgs e)
        {
            Device dev = (Device)sender;
            dev.RenderState.ZBufferEnable = true;		 	//允许使用深度缓冲
            dev.RenderState.Ambient = System.Drawing.Color.White;//设定环境光为白色
            dev.Lights[0].Type = LightType.Directional;  	//设置灯光类型
            dev.Lights[0].Diffuse = Color.White;			//设置灯光颜色
            dev.Lights[0].Direction = new Vector3(0, -1, 0);	//设置灯光位置
            dev.Lights[0].Update();						//更新灯光设置，创建第一盏灯光
            dev.Lights[0].Enabled = true;					//使设置有效
            dev.Material = meshMaterials;					//指定设备的材质

        }

        public void Render()		//渲染方法，本方法没有任何渲染代码，可认为是渲染方法的框架
        {
            if (device == null)
                return;
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.WhiteSmoke, 1.0f, 0);
            device.BeginScene();
            SetupMatrices();
            for (int i = 0; i < meshMaterials1.Length; i++)//Mesh中可能有多个3D图形，逐一显示
            {
                device.Material = meshMaterials1[i];//设定3D图形的材质
                device.SetTexture(0, meshTextures[i]);//设定3D图形的纹理
                mesh.DrawSubset(i);//显示该3D图形
            }
            device.EndScene();
            device.Present();

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            this.Render();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            pause = ((this.WindowState == FormWindowState.Minimized) || !this.Visible);
        }

        void SetupMatrices()
        {
            device.Transform.World = Matrix.RotationY(Angle);//世界变换，下条为观察变换矩阵
            device.Transform.View = Matrix.LookAtLH(new Vector3(0.0f, 3.0f, ViewZ),
         new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)(Math.PI / 4),
         1.0f, 1.0f, 100.0f);   //设置投影变换矩阵
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)		//e.KeyCode是键盘每个键的编号
            {
                case Keys.Left:		//Keys.Left是左箭头键编号，老虎左移
                    Angle += 0.1F;
                    break;
                case Keys.Right:		//老虎右移
                    Angle -= 0.1F;
                    break;
                case Keys.Down:		//老虎变小
                    ViewZ += 1;
                    break;
                case Keys.Up:		//老虎变大
                    ViewZ -= 1;
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

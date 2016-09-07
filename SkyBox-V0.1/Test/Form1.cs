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

namespace Test
{
    public partial class Form1 : Form
    {
        private Device device = null;
        bool pause = false;
        VertexBuffer vertexBuffer = null;
        Material mtrl;
        float Angle = 0, ViewZ = -6.0f;
        Texture frontTexture = null;//定义6个面的纹理变量
        Texture backTexture = null;
        Texture topTexture = null;
        Texture bottomTexture=null;
        Texture leftTexture = null;
        Texture rightTexture = null;

        private float angleY = 0.01f;//定义绕Y轴旋转变量

        private int mouseLastX, mouseLastY;//记录鼠标按下时的坐标位置
        private bool isRotateByMouse = false;//记录是否由鼠标控制旋转
        private bool isMoveByMouse = false;//记录是否由鼠标控制移动

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
                device = new Device(0, DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, presentParams);
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
            Device dev = (Device)sender;		//注意阴影部分
            vertexBuffer = new VertexBuffer(
                typeof(CustomVertex.PositionNormalTextured), 
                6, 
                dev, 
                0, 
                CustomVertex.PositionNormalTextured.Format, 
                Pool.Default);
            vertexBuffer.Created += new System.EventHandler(this.OnCreateVertexBuffer);
            this.OnCreateVertexBuffer(vertexBuffer, null);
            
            mtrl = new Material();
            mtrl.Diffuse = System.Drawing.Color.Yellow;		//物体的颜色
            mtrl.Ambient = System.Drawing.Color.Red;			//反射环境光的颜色
            frontTexture = TextureLoader.FromFile(dev, @"F:\\workdir\\VC# Based DirectX\\skybox\\neg_z.bmp");
            backTexture = TextureLoader.FromFile(dev, @"F:\\workdir\\VC# Based DirectX\\skybox\\pos_z.bmp");
            topTexture = TextureLoader.FromFile(dev, @"F:\\workdir\\VC# Based DirectX\\skybox\\pos_y.bmp");
            bottomTexture = TextureLoader.FromFile(dev, @"F:\\workdir\\VC# Based DirectX\\skybox\\neg_y.bmp");
            leftTexture = TextureLoader.FromFile(dev, @"F:\\workdir\\VC# Based DirectX\\skybox\\pos_x.bmp");
            rightTexture = TextureLoader.FromFile(dev, @"F:\\workdir\\VC# Based DirectX\\skybox\\neg_x.bmp");
        }
        public void OnCreateVertexBuffer(object sender, EventArgs e)
        {
            CustomVertex.PositionNormalTextured[] verts =
                        (CustomVertex.PositionNormalTextured[])vertexBuffer.Lock(0, 0);
            verts[0].Position = new Vector3(-10.0f, -10.0f, 0.0f);	  //顶点0位置，注意为Vector3
            verts[0].Normal = new Vector3(0, 0, -1);    	  		  //顶点0法线
            verts[0].Tu = 0.0f;    	  						  	  //顶点0纹理坐标Tu
            verts[0].Tv = 1.0f;
            verts[1].Position = new Vector3(-10.0f, 10.0f, 0.0f);	  	  //顶点1位置
            verts[1].Normal = new Vector3(0, 0, -1);				  //顶点1法线
            verts[1].Tu = 0.0f;    	  							  //顶点0纹理坐标Tu
            verts[1].Tv = 0.0f;
            verts[2].Position = new Vector3(10.0f, 10.0f, 0.0f);	      //顶点2位置
            verts[2].Normal = new Vector3(0, 0, -1);
            verts[2].Tu = 1.0f;    	  							  //顶点0纹理坐标Tu
            verts[2].Tv = 0.0f;
            verts[3].Position = new Vector3(-10.0f, -10.0f, 0.0f);	  //顶点3位置
            verts[3].Normal = new Vector3(0, 0, -1);    	  		  //顶点3法线
            verts[3].Tu = 0.0f;    	  							  //顶点0纹理坐标Tu
            verts[3].Tv = 1.0f;
            verts[4].Position = new Vector3(10.0f, 10.0f, 0.0f);	  	  //顶点4位置
            verts[4].Normal = new Vector3(0, 0, -1);
            verts[4].Tu = 1.0f;    	  							  //顶点0纹理坐标Tu
            verts[4].Tv = 0.0f;
            verts[5].Position = new Vector3(10.0f, -10.0f, 0.0f);	      //顶点5位置
            verts[5].Normal = new Vector3(0, 0, -1);
            verts[5].Tu = 1.0f;    	  							  //顶点0纹理坐标Tu
            verts[5].Tv = 1.0f;
            vertexBuffer.Unlock();
        }

        public void OnResetDevice(object sender, EventArgs e)
        {
            Device dev = (Device)sender;
            dev.RenderState.CullMode = Cull.CounterClockwise;		//没有背面剔除
            device.RenderState.ZBufferEnable = true;				//打开Z缓冲
            device.RenderState.Lighting = false;					//打开灯光
            //SetupLights();										//设置灯光
            device.SamplerState[0].MagFilter = TextureFilter.Linear;//线性滤波，结合处容易出现裂缝
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
            device.RenderState.CullMode = Cull.None;

            device.SetStreamSource(0, vertexBuffer, 0);
            device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
            
            device.SetTexture(0, frontTexture);
            device.Transform.World = 
                Matrix.Translation(0, 0, -10);//沿Z轴向观察者方向移动10个单位
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);//正前面

            device.SetTexture(0, backTexture);
            device.Transform.World = 
                Matrix.RotationY((float)Math.PI) * Matrix.Translation(0, 0, 10);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);//正后面

            device.SetTexture(0, rightTexture);
            device.Transform.World =
         Matrix.RotationY(-(float)Math.PI / 2) * Matrix.Translation(10, 0, 0);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);//右侧面

            device.SetTexture(0, leftTexture);
            device.Transform.World =
         Matrix.RotationY((float)Math.PI / 2) * Matrix.Translation(-10, 0, 0);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);//左侧面

            device.SetTexture(0, topTexture);
            device.Transform.World =
         Matrix.RotationX((float)Math.PI / 2) * Matrix.RotationY(-(float)Math.PI / 2) * Matrix.Translation(0, 10, 0);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);//上面

            device.SetTexture(0, bottomTexture);
            device.Transform.World =
         Matrix.RotationX(-(float)Math.PI / 2) * Matrix.RotationY(-(float)Math.PI/2)*Matrix.Translation(0, -10, 0);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);//下面

            device.EndScene();			//渲染结束
            device.Present();	//更新显示区域，把后备缓存的D图形送到图形卡的显存中显示

        }
        private Vector3 CamPostion = new Vector3(0.0f, 0.0f, -5.0f);//定义摄像机位置
        private Vector3 CamTarget = new Vector3(0.0f, 0.0f, 0.0f);//定义摄像机目标位置
        private void SetupMatrices()		//注意世界变换和观察变换参数可能要改变
        {
//             device.Transform.World = Matrix.RotationY(0);	//世界变换矩阵
//             Vector3 v1 = new Vector3(0.0f, 0.0f, -5.0f);		//下句使v1点分别沿Y轴和X轴旋转
//             v1.TransformCoordinate(Matrix.RotationYawPitchRoll(Angle, ViewZ, 0));
//             device.Transform.View = Matrix.LookAtLH(v1, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));	//观察变换矩阵
//             device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4,
//                             this.Width/this.Height, 1.0f, 100.0f);			//投影变换矩阵
            Matrix viewMatrix = Matrix.LookAtLH(CamPostion, CamTarget, new Vector3(0, 1, 0));
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, this.Width / this.Height, 1.0f, 6500.0f);
            device.Transform.View = viewMatrix;
        }

        private void SetupLights()
        {
            device.Material = mtrl;
            device.Lights[0].Type = LightType.Directional;			//灯光类型
            device.Lights[0].Diffuse = System.Drawing.Color.White;	//光的颜色为白色
            device.Lights[0].Direction = new Vector3(0, -2, 4);//灯光方向从观察者上方指向屏幕下方
            device.Lights[0].Update();			//更新灯光设置，创建第一盏灯光
            device.Lights[0].Enabled = true;		//使设置有效，下句设置环境光为白色
            device.RenderState.Ambient = System.Drawing.Color.FromArgb(0x808080);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeGraphics();
            this.Show();
            Render();
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            this.Render();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            pause = ((this.WindowState == FormWindowState.Minimized) || !this.Visible);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            Vector4 tempV4;
            Matrix currentView = device.Transform.View;//当前摄像机的视图矩阵
            switch (e.KeyCode)
            {
                case Keys.Left:
                    CamPostion.Subtract(CamTarget);
                    tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                            Quaternion.RotationAxis(new Vector3(currentView.M12, currentView.M22, currentView.M32), -angleY)));
                    CamPostion.X = tempV4.X + CamTarget.X;
                    CamPostion.Y = tempV4.Y + CamTarget.Y;
                    CamPostion.Z = tempV4.Z + CamTarget.Z;
                    break;
                case Keys.Right:
                    CamPostion.Subtract(CamTarget);
                    tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                            Quaternion.RotationAxis(new Vector3(currentView.M12, currentView.M22, currentView.M32), angleY)));
                    CamPostion.X = tempV4.X + CamTarget.X;
                    CamPostion.Y = tempV4.Y + CamTarget.Y;
                    CamPostion.Z = tempV4.Z + CamTarget.Z;
                    break;
                case Keys.Up:
                    CamPostion.Subtract(CamTarget);
                    tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                       Quaternion.RotationAxis(new Vector3(device.Transform.View.M11
                       , device.Transform.View.M21, device.Transform.View.M31), -angleY)));
                    CamPostion.X = tempV4.X + CamTarget.X;
                    CamPostion.Y = tempV4.Y + CamTarget.Y;
                    CamPostion.Z = tempV4.Z + CamTarget.Z;
                    break;
                case Keys.Down:
                    CamPostion.Subtract(CamTarget);
                    tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                       Quaternion.RotationAxis(new Vector3(device.Transform.View.M11
                       , device.Transform.View.M21, device.Transform.View.M31), angleY)));
                    CamPostion.X = tempV4.X + CamTarget.X;
                    CamPostion.Y = tempV4.Y + CamTarget.Y;
                    CamPostion.Z = tempV4.Z + CamTarget.Z;
                    break;
                case Keys.Add:
                    CamPostion.Subtract(CamTarget);
                    CamPostion.Scale(0.95f);
                    CamPostion.Add(CamTarget);
                    break;
                case Keys.Subtract:
                    CamPostion.Subtract(CamTarget);
                    CamPostion.Scale(1.05f);
                    CamPostion.Add(CamTarget);
                    break;
            }
            Matrix viewMatrix = Matrix.LookAtLH(CamPostion, CamTarget, new Vector3(0, 1, 0));
            device.Transform.View = viewMatrix;
            Render();
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseLastX = e.X;
                mouseLastY = e.Y;
                isRotateByMouse = true;
            }
            else if (e.Button == MouseButtons.Middle)
            {
                mouseLastX = e.X;
                mouseLastY = e.Y;
                isMoveByMouse = true;
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            isRotateByMouse = false;
            isMoveByMouse = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isRotateByMouse)
            {
                Matrix currentView = device.Transform.View;//当前摄像机的视图矩阵
                float tempAngleY = 2 * (float)(e.X - mouseLastX) / this.Width;
                CamPostion.Subtract(CamTarget);

                Vector4 tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                    Quaternion.RotationAxis(new Vector3(currentView.M12, currentView.M22, currentView.M32), tempAngleY)));
                CamPostion.X = tempV4.X;
                CamPostion.Y = tempV4.Y;
                CamPostion.Z = tempV4.Z;

                float tempAngleX = 4 * (float)(e.Y - mouseLastY) / this.Height;
                tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                    Quaternion.RotationAxis(new Vector3(currentView.M11, currentView.M21, currentView.M31), tempAngleX)));
                CamPostion.X = tempV4.X + CamTarget.X;
                CamPostion.Y = tempV4.Y + CamTarget.Y;
                CamPostion.Z = tempV4.Z + CamTarget.Z;
                Matrix viewMatrix = Matrix.LookAtLH(CamPostion, CamTarget, new Vector3(0, 1, 0));
                device.Transform.View = viewMatrix;

                mouseLastX = e.X;
                mouseLastY = e.Y;
                Render();
            }
            else if (isMoveByMouse)
            {
                Matrix currentView = device.Transform.View;//当前摄像机的视图矩阵
                float moveFactor = 0.01f;
                CamTarget.X += -moveFactor * ((e.X - mouseLastX) * currentView.M11 - (e.Y - mouseLastY) * currentView.M12);
                CamTarget.Y += -moveFactor * ((e.X - mouseLastX) * currentView.M21 - (e.Y - mouseLastY) * currentView.M22);
                CamTarget.Z += -moveFactor * ((e.X - mouseLastX) * currentView.M31 - (e.Y - mouseLastY) * currentView.M32);

                CamPostion.X += -moveFactor * ((e.X - mouseLastX) * currentView.M11 - (e.Y - mouseLastY) * currentView.M12);
                CamPostion.Y += -moveFactor * ((e.X - mouseLastX) * currentView.M21 - (e.Y - mouseLastY) * currentView.M22);
                CamPostion.Z += -moveFactor * ((e.X - mouseLastX) * currentView.M31 - (e.Y - mouseLastY) * currentView.M32);

                Matrix viewMatrix = Matrix.LookAtLH(CamPostion, CamTarget, new Vector3(0, 1, 0));
                device.Transform.View = viewMatrix;
                mouseLastX = e.X;
                mouseLastY = e.Y;
                Render();
            }
        }
        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            float scaleFactor = -(float)e.Delta / 2000 + 1f;
            CamPostion.Subtract(CamTarget);
            CamPostion.Scale(scaleFactor);
            CamPostion.Add(CamTarget);
            Matrix viewMatrix = Matrix.LookAtLH(CamPostion, CamTarget, new Vector3(0, 1, 0));
            device.Transform.View = viewMatrix;
            Render();
        }
    }
}

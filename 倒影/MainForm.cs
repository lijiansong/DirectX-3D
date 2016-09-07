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

namespace 倒影
{
    public partial class MainForm : Form
    {
        private Device device = null;
        bool pause = false;
        Mesh mesh = null;								//mesh引用变量，将引用生成的茶壶
        Material meshMaterials, mtr;					//分别是茶壶和地板使用的材质引用变量
        VertexBuffer vertexBuffer = null;				//保存建立地板正方形的顶点
        //float angle = 0.0f;								//茶壶旋转的角度
        Texture texture = null;							//地板纹理引用变量

        public Vector3 CamPostion = new Vector3(0.0f, 1.0f, -5.0f);//定义摄像机位置
        public Vector3 CamTarget = new Vector3(0.0f, 0.0f, 0.0f);//定义摄像机目标位置

        private float angleY = 0.01f;//定义绕Y轴旋转变量

        private int mouseLastX, mouseLastY;//记录鼠标按下时的坐标位置
        private bool isRotateByMouse = false;//记录是否由鼠标控制旋转
        private bool isMoveByMouse = false;//记录是否由鼠标控制移动


        public MainForm()
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
            mesh = Mesh.Teapot(device);					//增加茶壶
            meshMaterials = new Material();				//建立茶壶使用的材质对象
            meshMaterials.Ambient = Color.White;					//材质如何反射环境光
            meshMaterials.Diffuse = Color.White;					//材质如何反射灯光
            mtr = new Material();								//建立地板使用的材质对象
            //材质反射环境光和漫反射光的颜色为透明白色，注意Alpha=200
            mtr.Ambient = Color.FromArgb(200, 255, 255, 255);
            mtr.Diffuse = Color.FromArgb(200, 255, 255, 255);
            texture = TextureLoader.FromFile(device, @"..\..\..\p1.bmp");  //地板纹理图案
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionNormalTextured), 6, device, 0, CustomVertex.PositionNormalTextured.Format, Pool.Default);
            vertexBuffer.Created += new System.EventHandler(this.OnCreateVertexBuffer);
            this.OnCreateVertexBuffer(vertexBuffer, null);
        }

        public void OnCreateVertexBuffer(object sender, EventArgs e)
        {
            CustomVertex.PositionNormalTextured[] verts = (CustomVertex.PositionNormalTextured[])vertexBuffer.Lock(0, 0);		//绘制正方形需要6个顶点     
            verts[0].Position = new Vector3(-10.0f, 0.0f, -10.0f);	//顶点0位置，注意为Vector3
            verts[0].Normal = new Vector3(0, 0, -1);
            verts[0].Tu = 0.0f;    	  //顶点0纹理坐标Tu
            verts[0].Tv = 20.0f;		  //参见8.4节，纹理图片沿Y轴方向重复贴图20次
            verts[1].Position = new Vector3(-10.0f, 0.0f, 10.0f);	  	  		//顶点1位置
            verts[1].Normal = new Vector3(0, 0, -1);
            verts[1].Tu = 0.0f;    	  //顶点1纹理坐标Tu
            verts[1].Tv = 0.0f;
            verts[2].Position = new Vector3(10.0f, 0.0f, 10.0f);	      		//顶点2位置
            verts[2].Normal = new Vector3(0, 0, -1);
            verts[2].Tu = 20.0f;    	  //顶点2纹理坐标Tu
            verts[2].Tv = 0.0f;
            verts[3].Position = new Vector3(-10.0f, 0.0f, -10.0f);	  		//顶点3位置
            verts[3].Normal = new Vector3(0, 0, -1);
            verts[3].Tu = 0.0f;    	  //顶点3纹理坐标Tu
            verts[3].Tv = 20.0f;
            verts[4].Position = new Vector3(10.0f, 0.0f, 10.0f);	  	  		//顶点5位置
            verts[4].Normal = new Vector3(0, 0, -1);
            verts[4].Tu = 20.0f;    	  //顶点4纹理坐标Tu
            verts[4].Tv = 0.0f;
            verts[5].Position = new Vector3(10.0f, 0.0f, -10.0f);	      		//顶点6位置
            verts[5].Normal = new Vector3(0, 0, -1);
            verts[5].Tu = 20.0f;    	  //顶点5纹理坐标Tu
            verts[5].Tv = 20.0f;
            vertexBuffer.Unlock();
        }


        public void OnResetDevice(object sender, EventArgs e)
        {
            

            Device dev = (Device)sender;
            dev.RenderState.ZBufferEnable = true;	 		//允许使用深度缓冲，意义见5.2节
            dev.RenderState.Ambient = Color.Black;	 	//环境光为黑色
            dev.Lights[0].Type = LightType.Directional;  	//设置灯光类型为定向光
            dev.Lights[0].Diffuse = Color.White;		 	//设置定向灯光颜色为白色
            dev.Lights[0].Direction = new Vector3(0, -1, 4);	//灯光方向从(0,0,0)到(0,-1,4)
            dev.Lights[0].Update();						//更新灯光设置，创建第一盏灯光
            dev.Lights[0].Enabled = true;				//使设置有效
            //dev.Material = meshMaterials;				//指定设备的材质
            device.Transform.View = Matrix.LookAtLH(new Vector3(0.0f, 1.0f, -5.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
            device.Transform.Projection = Matrix.PerspectiveFovLH(
                (float)(Math.PI / 4),
                this.Width / this.Height,
                1.0f, 100.0f);


        }

        public void Render()		//渲染方法，本方法没有任何渲染代码，可认为是渲染方法的框架
        {
            if (device == null) 			//如果未建立设备对象，退出
                return;
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.WhiteSmoke, 1.0f, 0);
            device.BeginScene();			//开始渲染
            //下句设置上方茶壶的世界变换矩阵
            device.Transform.World = Matrix.RotationYawPitchRoll(angleY, angleY, 0)		//茶壶旋转
     * Matrix.Translation(0.0f, 1.5f, 5.0f); //世界变换，y=0平面上方1.5f绘制茶壶
            device.Material = meshMaterials;
            mesh.DrawSubset(0);		//绘制上方的茶壶
            //设置倒影茶壶世界变换矩阵，Matrix.Scaling(1,-1,1)将3D物体的Y轴坐标*(-1)
            //Matrix.Translation(0.0f, -1.5f, 5.0f),表示y=0平面下方1.5f绘制茶壶
            device.Transform.World = Matrix.RotationYawPitchRoll(angleY, angleY, 0) *  //茶壶旋转
     Matrix.Scaling(1, -1, 1) * Matrix.Translation(0.0f, -1.5f, 5.0f);
            device.Lights[0].Direction = new Vector3(0, 1, 4);   //灯光方向从(0,0,0)到(0,1,4)
            device.RenderState.CullMode = Cull.Clockwise;			//只绘制逆时针三角形
            mesh.DrawSubset(0);									//绘制下边的茶壶
            device.Lights[0].Direction = new Vector3(0, -1, 4);		//恢复灯光方向
            device.RenderState.CullMode = Cull.CounterClockwise;		//恢复背面剔除
            device.Transform.World = Matrix.Translation(0.0f, 0.0f, 0.0f);		//世界变换
            device.Material = mtr;								//地板使用的材质
            StateBlock sb = new StateBlock(device, StateBlockType.All);
            sb.Capture();//使用sb保存设置会降低运行速度，不建议使用，可采用使用后逐项恢复的方法
            device.RenderState.DiffuseMaterialSource = ColorSource.Material;
            device.RenderState.AlphaBlendEnable = true;
            device.SetTexture(0, texture);
            device.TextureState[0].ColorOperation = TextureOperation.Modulate;
            device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
            device.TextureState[0].ColorArgument2 = TextureArgument.Diffuse;
            device.TextureState[0].AlphaOperation = TextureOperation.Modulate;
            device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
            device.TextureState[0].AlphaArgument2 = TextureArgument.Diffuse;
            device.RenderState.SourceBlend = Blend.SourceColor;
            device.RenderState.DestinationBlend = Blend.InvSourceAlpha;
            device.SetStreamSource(0, vertexBuffer, 0);
            device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            sb.Apply();
            device.EndScene();			//渲染结束
            device.Present();//更新显示区域，把后备缓存的3D图形送到图形卡的屏幕显示区中显示

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            this.Render();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            pause = ((this.WindowState == FormWindowState.Minimized) || !this.Visible);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeGraphics();
            this.Show();
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

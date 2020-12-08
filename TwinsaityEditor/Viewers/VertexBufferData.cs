using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.InteropServices;

namespace TwinsanityEditor
{
    public class VertexBufferData
    {
        public int ID { get; set; }
        public int LastSize { get; set; }
        public int[] VtxOffs { get; set; }
        public int[] VtxCounts { get; set; }
        public uint[] VtxInd { get; set; }
        public Vertex[] Vtx { get; set; }
        public BufferType Type { get; set; }
        public int LinkID { get; set; }
        public int LayerID { get; set; }
        public int Tex { get; set; }

        public enum BufferType
        {
            Undefined = 0,
            Object = 1,
            Skydome = 2,
            Scenery = 3,
            Collision = 4,
            ExtraScenery = 5,
            ExtraCollision = 6,
            ExtraObject = 7,
        }

        public VertexBufferData()
        {
            ID = GL.GenBuffer();
            LastSize = 0;
            Type = BufferType.Undefined;
        }

        public void DrawAll(PrimitiveType primitive_type, BufferPointerFlags flags)
        {
            draw_func(0, primitive_type, flags);
        }

        public void DrawAllElements(PrimitiveType primitive_type, BufferPointerFlags flags)
        {
            draw_func(1, primitive_type, flags);
        }

        public void DrawMulti(PrimitiveType primitive_type, BufferPointerFlags flags)
        {
            draw_func(2, primitive_type, flags);
        }

        private void draw_func(int func, PrimitiveType prim, BufferPointerFlags flags)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
            GL.VertexPointer(3, VertexPointerType.Float, Vertex.SizeOf, Vertex.OffsetOfPos);
            if ((flags & BufferPointerFlags.Normal) != 0)
            {
                GL.EnableClientState(ArrayCap.NormalArray);
                GL.NormalPointer(NormalPointerType.Float, Vertex.SizeOf, Vertex.OffsetOfNor);
            }
            if ((flags & BufferPointerFlags.TexCoord) != 0)
            {
                GL.EnableClientState(ArrayCap.TextureCoordArray);
                GL.TexCoordPointer(2, TexCoordPointerType.Float, Vertex.SizeOf, Vertex.OffsetOfTex);

                //GL.BufferData(BufferTarget.ArrayBuffer, Vertex.SizeOf * 128, Vtx, BufferUsageHint.DynamicDraw);
                //GL.BindTexture(TextureTarget.Texture2D, Tex);

                /*
                foreach (var k in charVtx.Keys)
                {
                    if (charVtxOffs[k] == 0) continue;
                    if (charVtxBufLen < charVtx[k].Length)
                    {
                        GL.BufferData(BufferTarget.ArrayBuffer, Vertex.SizeOf * charVtx[k].Length, charVtx[k], BufferUsageHint.DynamicDraw);
                        charVtxBufLen = charVtx[k].Length;
                    }
                    else
                    {
                        GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, Vertex.SizeOf * charVtxOffs[k], charVtx[k]);
                    }
                    GL.BindTexture(TextureTarget.Texture2D, textureCharMap[k]);
                }
                */
                //GL.BindTexture(TextureTarget.Texture2D, 0);
            }
            if ((flags & BufferPointerFlags.Color) != 0)
            {
                GL.EnableClientState(ArrayCap.ColorArray);
                GL.ColorPointer(4, ColorPointerType.UnsignedByte, Vertex.SizeOf, Vertex.OffsetOfCol);
            }
            switch (func)
            {
                case 0:
                    GL.DrawArrays(prim, 0, Vtx.Length);
                    break;
                case 1:
                    GL.DrawElements(prim, VtxInd.Length, DrawElementsType.UnsignedInt, VtxInd);
                    break;
                case 2:
                    GL.MultiDrawArrays(prim, VtxOffs, VtxCounts, VtxCounts.Length);
                    break;
            }
            if ((flags & BufferPointerFlags.Normal) != 0)
                GL.DisableClientState(ArrayCap.NormalArray);
            if ((flags & BufferPointerFlags.TexCoord) != 0)
                GL.DisableClientState(ArrayCap.TextureCoordArray);
            if ((flags & BufferPointerFlags.Color) != 0)
                GL.DisableClientState(ArrayCap.ColorArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }

    [Flags]
    public enum BufferPointerFlags
    {
        None = 0,
        NormalNoCol = 1,
        TexCoordNoCol = 2,
        Color = 4,
        Normal,
        TexCoord,
        Default = Color
    }
}

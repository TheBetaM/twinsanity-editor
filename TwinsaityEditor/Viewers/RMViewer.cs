using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Twinsanity;

namespace TwinsanityEditor
{
    public class RMViewer : ThreeDViewer
    {
        private static readonly int circle_res = 16;
        private static readonly int reserved_layers = 7;
        private int static_layers = 0;
        private int scenery_starting_layer = 0; //Temporary!! while there can only be a fixed amount of scenery
        private int scenery_layer = 0; //ditto

        private bool show_col_nodes, show_triggers, show_cams, wire_col, sm2_links, obj_models, show_col, show_inst, show_pos, show_hurtzones, show_invis, show_scenery, show_skydome, show_linked_chunks;
        private FileController file;
        private ChunkLinks links;

        private List<DefaultEnums.SurfaceTypes> Surf_Walkable = new List<DefaultEnums.SurfaceTypes>()
        {
            DefaultEnums.SurfaceTypes.SURF_DEFAULT, DefaultEnums.SurfaceTypes.SURF_GENERIC_MEDIUM_SLIPPY, DefaultEnums.SurfaceTypes.SURF_GENERIC_MEDIUM_SLIPPY_RIGID_ONLY, DefaultEnums.SurfaceTypes.SURF_GENERIC_SLIGHTLY_SLIPPY, DefaultEnums.SurfaceTypes.SURF_GLASS_WALL,
            DefaultEnums.SurfaceTypes.SURF_HACK_RAIL, DefaultEnums.SurfaceTypes.SURF_ICE, DefaultEnums.SurfaceTypes.SURF_ICE_LOW_SLIPPY, DefaultEnums.SurfaceTypes.SURF_NORMAL_GRASS, DefaultEnums.SurfaceTypes.SURF_NORMAL_METAL,
            DefaultEnums.SurfaceTypes.SURF_NORMAL_MUD, DefaultEnums.SurfaceTypes.SURF_NORMAL_ROCK, DefaultEnums.SurfaceTypes.SURF_NORMAL_SAND, DefaultEnums.SurfaceTypes.SURF_NORMAL_SNOW,
            DefaultEnums.SurfaceTypes.SURF_NORMAL_STONE_TILES, DefaultEnums.SurfaceTypes.SURF_NORMAL_WATER, DefaultEnums.SurfaceTypes.SURF_NORMAL_WOOD, DefaultEnums.SurfaceTypes.SURF_SLIPPY_METAL,
            DefaultEnums.SurfaceTypes.SURF_SLIPPY_ROCK, DefaultEnums.SurfaceTypes.SURF_STICKY_SNOW
        };
        private List<DefaultEnums.SurfaceTypes> Surf_Hurt = new List<DefaultEnums.SurfaceTypes>()
        {
            DefaultEnums.SurfaceTypes.SURF_DROWNING_PLANE, DefaultEnums.SurfaceTypes.SURF_FALL_THRU_DEATH, DefaultEnums.SurfaceTypes.SURF_GENERIC_INSTANT_DEATH, DefaultEnums.SurfaceTypes.SURF_LAVA, DefaultEnums.SurfaceTypes.SURF_NONSOLID_ELECTRIC_DEATH,
        };
        private List<DefaultEnums.SurfaceTypes> Surf_Invis = new List<DefaultEnums.SurfaceTypes>()
        {
             DefaultEnums.SurfaceTypes.SURF_BLOCK_AI_ONLY, DefaultEnums.SurfaceTypes.SURF_BLOCK_PLAYER, DefaultEnums.SurfaceTypes.SURF_CAMERA_BLOCKING,
        };

        private List<DefaultEnums.ObjectID> WoodCrates = new List<DefaultEnums.ObjectID>()
        {
            DefaultEnums.ObjectID.AKUAKUCRATE, DefaultEnums.ObjectID.AMMOCRATESMALL, DefaultEnums.ObjectID.BASICCRATE, DefaultEnums.ObjectID.CHECKPOINTCRATE, DefaultEnums.ObjectID.EXTRALIFECRATE, DefaultEnums.ObjectID.EXTRALIFECRATECORTEX, DefaultEnums.ObjectID.EXTRALIFECRATENINA,
            DefaultEnums.ObjectID.LEVELCRATE, DefaultEnums.ObjectID.MULTIPLEHITCRATE, DefaultEnums.ObjectID.SURPRISECRATE, DefaultEnums.ObjectID.WOODENSPRINGCRATE
        };

        public RMViewer(FileController file, ref Form pform)
        {
            //initialize variables here
            show_col_nodes = show_triggers = wire_col = show_cams = show_scenery = show_linked_chunks = false;
            sm2_links = obj_models = show_col = show_inst = show_hurtzones = show_pos = show_skydome = show_invis = show_hud = true;
            this.file = file;
            Tag = pform;
            static_layers = 0;
            int ObjectModelPool = 4000; // model limit
            InitVBO(ObjectModelPool, false);
            if (file.DataAux != null && file.DataAux.ContainsItem(5))
            {
                links = file.DataAux.GetItem<ChunkLinks>(5);
            }
            if (file.Data.ContainsItem(9))
            {
                if (file.Data.GetItem<ColData>(9).Size >= 12)
                {
                    for (int i = 0; i < 28; i++)
                    {
                        pform.Text = "Loading collision tree (" + i + ")...";
                        LoadColTree(file, i, false, new Matrix4(), 0);
                    }
                    pform.Text = "Loading collision nodes...";
                    LoadColNodes();
                }
            }
            if (file.DataAux != null)
            {
                SceneryDataController scenery_sec = file.AuxCont.GetItem<SceneryDataController>(0);
                SectionController skydome_sec = file.AuxCont.GetItem<SectionController>(6).GetItem<SectionController>(8);
                if (file.DataAux.Type != TwinsFile.FileType.SMX) // needs xbox model reading
                {
                    if (skydome_sec.Data.ContainsItem(scenery_sec.Data.SkydomeID))
                    {
                        pform.Text = "Loading skydome...";
                        LoadSkydome();
                    }
                    scenery_starting_layer = reserved_layers + static_layers;
                    scenery_layer = 0;
                    pform.Text = "Loading scenery...";
                    LoadScenery(file.AuxCont, false, new Matrix4(), 0, false);
                    pform.Text = "Loading dynamic scenery...";
                    LoadDynamicScenery(file.AuxCont, false, new Matrix4(), 0);
                }
                pform.Text = "Loading lights...";
                LoadLights();
                if (links.Links.Count > 0)
                {
                    pform.Text = "Loading linked chunks...";
                    LoadAllLinkedChunks();
                }
            }
            pform.Text = "Loading particles...";
            LoadParticles();
            pform.Text = "Loading instances...";
            LoadInstances();
            pform.Text = "Loading positions...";
            LoadPositions();
            pform.Text = "Loading AI positions...";
            LoadAIPositions();
            pform.Text = "Initializing...";
        }

        protected override void RenderHUD()
        {
            if (!show_hud)
            {
                return;
            }
            base.RenderHUD();
            string longDisp = "";
            longDisp += " /: SCEN / ";
            if (show_scenery)
            {
                longDisp += "ON\n";
            }
            else
            {
                longDisp += "OFF\n";
            }
            longDisp += " 1: COLL / ";
            if (show_col)
            {
                longDisp += "ON\n";
            }
            else
            {
                longDisp += "OFF\n";
            }
            longDisp += " 2: HURT / ";
            if (show_hurtzones)
            {
                longDisp += "ON\n";
            }
            else
            {
                longDisp += "OFF\n";
            }
            longDisp += " 3: INVI / ";
            if (show_invis)
            {
                longDisp += "ON\n";
            }
            else
            {
                longDisp += "OFF\n";
            }
            longDisp += " 4: OBJE / ";
            if (obj_models)
            {
                longDisp += "ON\n";
            }
            else
            {
                longDisp += "OFF\n";
            }
            longDisp += " 5: CLIN / ";
            if (sm2_links)
            {
                longDisp += "ON\n";
            }
            else
            {
                longDisp += "OFF\n";
            }
            longDisp += " 6: TRIG / ";
            if (show_triggers)
            {
                longDisp += "ON\n";
            }
            else
            {
                longDisp += "OFF\n";
            }
            longDisp += " 7: CAMT / ";
            if (show_cams)
            {
                longDisp += "ON\n";
            }
            else
            {
                longDisp += "OFF\n";
            }
            longDisp += " 8: INST / ";
            if (show_inst)
            {
                longDisp += "ON\n";
            }
            else
            {
                longDisp += "OFF\n";
            }
            longDisp += " 9: POSI / ";
            if (show_pos)
            {
                longDisp += "ON\n";
            }
            else
            {
                longDisp += "OFF\n";
            }
            longDisp += " 0: SKYD / ";
            if (show_skydome)
            {
                longDisp += "ON\n";
            }
            else
            {
                longDisp += "OFF\n";
            }
            longDisp += " -: VLIN / ";
            if (show_linked_chunks)
            {
                longDisp += "ON\n";
            }
            else
            {
                longDisp += "OFF\n";
            }
            longDisp += " =: DISP / ";
            if (show_hud)
            {
                longDisp += "ON\n";
            }
            else
            {
                longDisp += "OFF\n";
            }
            longDisp += " {: WIRE / ";
            if (wire_col)
            {
                longDisp += "ON\n";
            }
            else
            {
                longDisp += "OFF\n";
            }
            longDisp += " }: COLN / ";
            if (show_col_nodes)
            {
                longDisp += "ON";
            }
            else
            {
                longDisp += "OFF";
            }

            RenderString2D(longDisp, 2, Height, 14, 18, Color.Black, TextAnchor.BotLeft);
            RenderString2D(longDisp, 0, Height - 2, 14, 18, Color.White, TextAnchor.BotLeft);
            //RenderString2D("X: " + (-pos.X) + "\nY: " + pos.Y + "\nZ: " + pos.Z, 0, Height - 24, 14, Color.White, TextAnchor.BotLeft);
        }

        public Vector3 GetViewerPos()
        {
            return new Vector3(-pos.X,pos.Y,pos.Z);
        }

        protected override void RenderObjects()
        {
            //put all object rendering code here
            //draw collision
            if (file.Data.ContainsItem(9))
            {
                if (show_col)
                {
                    GL.Enable(EnableCap.Lighting);
                    for (int i = 0; i < reserved_layers + static_layers; i++)
                    {
                        if (vtx[i] != null && (vtx[i].Type == VertexBufferData.BufferType.Collision || (show_linked_chunks && vtx[i].Type == VertexBufferData.BufferType.ExtraCollision)))
                        {
                            if (Surf_Walkable.Contains((DefaultEnums.SurfaceTypes)vtx[i].LayerID))
                            {
                                vtx[i].DrawAllElements(PrimitiveType.Triangles, BufferPointerFlags.Normal);
                            }
                        }
                    }
                    GL.Disable(EnableCap.Lighting);
                }

                if (show_hurtzones)
                {
                    GL.Enable(EnableCap.Lighting);
                    for (int i = 0; i < reserved_layers + static_layers; i++)
                    {
                        if (vtx[i] != null && (vtx[i].Type == VertexBufferData.BufferType.Collision || (show_linked_chunks && vtx[i].Type == VertexBufferData.BufferType.ExtraCollision)))
                        {
                            if (Surf_Hurt.Contains((DefaultEnums.SurfaceTypes)vtx[i].LayerID))
                            {
                                vtx[i].DrawAllElements(PrimitiveType.Triangles, BufferPointerFlags.Normal);
                            }
                        }
                    }
                    GL.Disable(EnableCap.Lighting);
                }

                if (show_invis)
                {
                    GL.Enable(EnableCap.Lighting);
                    for (int i = 0; i < reserved_layers + static_layers; i++)
                    {
                        if (vtx[i] != null && (vtx[i].Type == VertexBufferData.BufferType.Collision || (show_linked_chunks && vtx[i].Type == VertexBufferData.BufferType.ExtraCollision)))
                        {
                            if (Surf_Invis.Contains((DefaultEnums.SurfaceTypes)vtx[i].LayerID))
                            {
                                vtx[i].DrawAllElements(PrimitiveType.Triangles, BufferPointerFlags.Normal);
                            }
                        }
                    }
                    GL.Disable(EnableCap.Lighting);
                }

                if (wire_col)
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    GL.Color3(Color.Black);
                    for (int i = 6; i < reserved_layers; i++)
                    {
                        if (vtx[i] != null && (vtx[i].Type == VertexBufferData.BufferType.Collision || (show_linked_chunks && vtx[i].Type == VertexBufferData.BufferType.ExtraCollision)))
                        {
                            vtx[i].DrawAllElements(PrimitiveType.Triangles, BufferPointerFlags.NormalNoCol);
                        }
                    }
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }

                if (show_col_nodes)
                {
                    vtx[2].DrawMulti(PrimitiveType.LineStrip, BufferPointerFlags.Default);
                }
            }

            //object visuals
            if (file.Data.Type != TwinsFile.FileType.RMX && obj_models)
            {
                GL.Enable(EnableCap.Lighting);
                for (int i = reserved_layers + static_layers; i < vtx.Length; i++)
                {
                    if (vtx[i] != null && vtx[i].Type == VertexBufferData.BufferType.Object)
                    {
                        vtx[i].DrawAllElements(PrimitiveType.Triangles, BufferPointerFlags.Normal);
                    }
                }
                GL.Disable(EnableCap.Lighting);
            }

            //skydome
            if (file.Data.Type != TwinsFile.FileType.RMX && show_skydome)
            {
                GL.Enable(EnableCap.Lighting);
                for (int i = reserved_layers; i < reserved_layers + static_layers; i++)
                {
                    if (vtx[i] != null && vtx[i].Type == VertexBufferData.BufferType.Skydome)
                    {
                        vtx[i].DrawAllElements(PrimitiveType.Triangles, BufferPointerFlags.Normal);
                    }
                }
                GL.Disable(EnableCap.Lighting);
            }

            //scenery
            if (file.Data.Type != TwinsFile.FileType.RMX && show_scenery)
            {
                GL.Enable(EnableCap.Lighting);
                for (int i = reserved_layers; i < reserved_layers + static_layers; i++)
                {
                    if (vtx[i] != null && vtx[i].Type == VertexBufferData.BufferType.Scenery)
                    {
                        vtx[i].DrawAllElements(PrimitiveType.Triangles, BufferPointerFlags.Normal);
                    }
                }
                GL.Disable(EnableCap.Lighting);
            }

            //chunk link scenery
            if (file.Data.Type != TwinsFile.FileType.RMX && show_linked_chunks && show_scenery)
            {
                GL.Enable(EnableCap.Lighting);
                for (int i = reserved_layers; i < reserved_layers + static_layers; i++)
                {
                    if (vtx[i] != null && vtx[i].Type == VertexBufferData.BufferType.ExtraScenery)
                    {
                        vtx[i].DrawAllElements(PrimitiveType.Triangles, BufferPointerFlags.Normal);
                    }
                }
                GL.Disable(EnableCap.Lighting);
            }

            //instances
            if (show_inst)
            {
                vtx[1].DrawMulti(PrimitiveType.LineStrip, BufferPointerFlags.Default);
            }

            //positions + ai positions
            if (show_pos)
            {
                vtx[3].DrawMulti(PrimitiveType.LineLoop, BufferPointerFlags.Default);
                vtx[4].DrawMulti(PrimitiveType.LineLoop, BufferPointerFlags.Default);
            }

            //particle positions
            if (show_pos && vtx[5].VtxOffs != null && vtx[5].VtxOffs.Length > 0)
            {
                vtx[5].DrawMulti(PrimitiveType.LineLoop, BufferPointerFlags.Default);
            }

            //lights
            if (show_pos && vtx[6].VtxOffs != null && vtx[6].VtxOffs.Length > 0)
            {
                vtx[6].DrawMulti(PrimitiveType.LineLoop, BufferPointerFlags.Default);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.PushMatrix();
            
            for (uint i = 0; i <= 7; ++i)
            {
                if (file.Data.ContainsItem(i))
                {
                    Color cur_color;
                    if (show_pos)
                    {
                        if (file.Data.GetItem<TwinsSection>(i).ContainsItem(1)) //aipositions
                        {
                            foreach (AIPosition pos in file.Data.GetItem<TwinsSection>(i).GetItem<TwinsSection>(1).Records)
                            {
                                if (file.SelectedItem != pos)
                                {
                                    GL.PointSize(5);
                                    cur_color = colors[colors.Length - i * 2 - 2];
                                }
                                else
                                {
                                    GL.PointSize(10);
                                    cur_color = Color.White;
                                }
                                GL.Color3(cur_color);
                                GL.Begin(PrimitiveType.Points);
                                GL.Vertex3(-pos.Pos.X, pos.Pos.Y, pos.Pos.Z);
                                GL.End();
                                RenderString3D(pos.ID.ToString(), cur_color, -pos.Pos.X, pos.Pos.Y, pos.Pos.Z, ref identity_mat, pos.Pos.W / 3);
                            }
                        }

                        if (file.Data.GetItem<TwinsSection>(i).ContainsItem(2)) //aipaths
                        {
                            foreach (AIPath pth in file.Data.GetItem<TwinsSection>(i).GetItem<TwinsSection>(2).Records)
                            {
                                AIPosition pth_begin = file.GetAIPos(i, pth.Arg[0]);
                                AIPosition pth_end = file.GetAIPos(i, pth.Arg[1]);

                                if (file.SelectedItem != pth)
                                {
                                    GL.PointSize(5);
                                    GL.LineWidth(1);
                                    cur_color = colors[colors.Length - i * 2 - 2];
                                }
                                else
                                {
                                    GL.PointSize(10);
                                    GL.LineWidth(2);
                                    cur_color = Color.White;
                                }
                                RenderString3D(pth.ID.ToString(), cur_color, -(pth_begin.Pos.X + pth_end.Pos.X) / 2, (pth_begin.Pos.Y + pth_end.Pos.Y) / 2, (pth_begin.Pos.Z + pth_end.Pos.Z) / 2, ref identity_mat, 0.5F);
                                GL.Color3(cur_color);
                                GL.Begin(PrimitiveType.Lines);
                                GL.Vertex3(-pth_begin.Pos.X, pth_begin.Pos.Y, pth_begin.Pos.Z);
                                GL.Vertex3(-pth_end.Pos.X, pth_end.Pos.Y, pth_end.Pos.Z);
                                GL.End();
                            }
                        }

                        if (file.Data.GetItem<TwinsSection>(i).ContainsItem(3)) //positions
                        {
                            foreach (Position pos in file.Data.GetItem<TwinsSection>(i).GetItem<TwinsSection>(3).Records)
                            {
                                if (file.SelectedItem != pos)
                                {
                                    GL.PointSize(5);
                                    cur_color = colors[colors.Length - i * 2 - 1];
                                }
                                else
                                {
                                    GL.PointSize(10);
                                    cur_color = Color.White;
                                }
                                GL.Color3(cur_color);
                                GL.Begin(PrimitiveType.Points);
                                GL.Vertex3(-pos.Pos.X, pos.Pos.Y, pos.Pos.Z);
                                GL.End();
                                RenderString3D(pos.ID.ToString(), cur_color, -pos.Pos.X, pos.Pos.Y, pos.Pos.Z, ref identity_mat, 0.5F);
                            }
                        }

                        if (file.Data.GetItem<TwinsSection>(i).ContainsItem(4)) //paths
                        {
                            foreach (Path pth in file.Data.GetItem<TwinsSection>(i).GetItem<TwinsSection>(4).Records)
                            {
                                for (int k = 0; k < pth.Positions.Count; ++k)
                                {
                                    DrawAxes(-pth.Positions[k].X, pth.Positions[k].Y, pth.Positions[k].Z, 0.5f);
                                    if (file.SelectedItem != pth || file.SelectedItemArg != k)
                                        cur_color = colors[colors.Length - i * 2 - 1];
                                    else
                                        cur_color = Color.White;
                                    RenderString3D($"{pth.ID.ToString()}:{k}", cur_color, -pth.Positions[k].X, pth.Positions[k].Y, pth.Positions[k].Z, ref identity_mat, 0.5F);
                                }
                                if (file.SelectedItem != pth)
                                {
                                    GL.PointSize(5);
                                    GL.LineWidth(1);
                                }
                                else
                                {
                                    GL.PointSize(10);
                                    GL.LineWidth(2);
                                }
                                GL.Begin(PrimitiveType.LineStrip);
                                for (int k = 0; k < pth.Positions.Count; ++k)
                                {
                                    if (file.SelectedItem != pth || file.SelectedItemArg != k)
                                        GL.Color3(colors[colors.Length - i * 2 - 1]);
                                    else
                                        GL.Color3(Color.White);
                                    GL.Vertex3(-pth.Positions[k].X, pth.Positions[k].Y, pth.Positions[k].Z);
                                }
                                GL.End();
                            }
                        }
                    }

                    if (show_inst)
                    {
                        if (file.Data.GetItem<TwinsSection>(i).ContainsItem(6)) //instances
                        {
                            if (file.Data.Type != TwinsFile.FileType.DemoRM2)
                            {
                                foreach (Instance ins in file.Data.GetItem<TwinsSection>(i).GetItem<TwinsSection>(6).Records)
                                {
                                    Matrix3 rot_ins = Matrix3.Identity;
                                    rot_ins *= Matrix3.CreateRotationX(ins.RotX / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                                    rot_ins *= Matrix3.CreateRotationY(-ins.RotY / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                                    rot_ins *= Matrix3.CreateRotationZ(-ins.RotZ / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                                    if (file.SelectedItem == ins)
                                        cur_color = Color.White;
                                    else
                                        cur_color = colors[colors.Length - i * 2 - 1];
                                    RenderString3D(ins.ID.ToString(), cur_color, -ins.Pos.X, ins.Pos.Y, ins.Pos.Z, ref rot_ins);
                                }
                            }
                            else
                            {
                                foreach (InstanceDemo ins in file.Data.GetItem<TwinsSection>(i).GetItem<TwinsSection>(6).Records)
                                {
                                    Matrix3 rot_ins = Matrix3.Identity;
                                    rot_ins *= Matrix3.CreateRotationX(ins.RotX / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                                    rot_ins *= Matrix3.CreateRotationY(-ins.RotY / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                                    rot_ins *= Matrix3.CreateRotationZ(-ins.RotZ / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                                    if (file.SelectedItem == ins)
                                        cur_color = Color.White;
                                    else
                                        cur_color = colors[colors.Length - i * 2 - 1];
                                    RenderString3D(ins.ID.ToString(), cur_color, -ins.Pos.X, ins.Pos.Y, ins.Pos.Z, ref rot_ins);
                                }
                            }
                        }
                    }

                    if (show_triggers && file.Data.GetItem<TwinsSection>(i).ContainsItem(7))
                    {
                        foreach (Trigger trg in file.Data.GetItem<TwinsSection>(i).GetItem<TwinsSection>(7).Records)
                        {
                            GL.PushMatrix();
                            GL.Translate(-trg.Coords[1].X, trg.Coords[1].Y, trg.Coords[1].Z);
                            Quaternion quat = new Quaternion(trg.Coords[0].X, -trg.Coords[0].Y, -trg.Coords[0].Z, trg.Coords[0].W);
                            Matrix4 new_mat = Matrix4.CreateFromQuaternion(quat);
                            GL.MultMatrix(ref new_mat);


                            cur_color = file.SelectedItem == trg ? Color.White : colors[colors.Length - i * 2 - 1];
                            GL.DepthMask(false);
                            GL.Enable(EnableCap.Lighting);
                            GL.Color4(cur_color.R, cur_color.G, cur_color.B, (byte)95);
                            GL.Begin(PrimitiveType.QuadStrip);
                            GL.Vertex3(-trg.Coords[2].X, -trg.Coords[2].Y, -trg.Coords[2].Z);
                            GL.Vertex3(trg.Coords[2].X, -trg.Coords[2].Y, -trg.Coords[2].Z);
                            GL.Vertex3(-trg.Coords[2].X, trg.Coords[2].Y, -trg.Coords[2].Z);
                            GL.Vertex3(trg.Coords[2].X, trg.Coords[2].Y, -trg.Coords[2].Z);
                            GL.Vertex3(-trg.Coords[2].X, trg.Coords[2].Y, trg.Coords[2].Z);
                            GL.Vertex3(trg.Coords[2].X, trg.Coords[2].Y, trg.Coords[2].Z);
                            GL.Vertex3(-trg.Coords[2].X, -trg.Coords[2].Y, trg.Coords[2].Z);
                            GL.Vertex3(trg.Coords[2].X, -trg.Coords[2].Y, trg.Coords[2].Z);
                            GL.Vertex3(-trg.Coords[2].X, -trg.Coords[2].Y, -trg.Coords[2].Z);
                            GL.Vertex3(trg.Coords[2].X, -trg.Coords[2].Y, -trg.Coords[2].Z);
                            GL.End();
                            GL.Begin(PrimitiveType.Quads);

                            GL.Vertex3(-trg.Coords[2].X, -trg.Coords[2].Y, -trg.Coords[2].Z);
                            GL.Vertex3(-trg.Coords[2].X, -trg.Coords[2].Y, trg.Coords[2].Z);
                            GL.Vertex3(-trg.Coords[2].X, trg.Coords[2].Y, trg.Coords[2].Z);
                            GL.Vertex3(-trg.Coords[2].X, trg.Coords[2].Y, -trg.Coords[2].Z);

                            GL.Vertex3(trg.Coords[2].X, -trg.Coords[2].Y, -trg.Coords[2].Z);
                            GL.Vertex3(trg.Coords[2].X, -trg.Coords[2].Y, trg.Coords[2].Z);
                            GL.Vertex3(trg.Coords[2].X, trg.Coords[2].Y, trg.Coords[2].Z);
                            GL.Vertex3(trg.Coords[2].X, trg.Coords[2].Y, -trg.Coords[2].Z);

                            GL.End();
                            GL.DepthMask(true);
                            GL.Disable(EnableCap.Lighting);

                            GL.Color4(cur_color);
                            GL.LineWidth(1);

                            GL.Begin(PrimitiveType.LineStrip);
                            GL.Vertex3(-trg.Coords[2].X, trg.Coords[2].Y, -trg.Coords[2].Z);
                            GL.Vertex3(trg.Coords[2].X, trg.Coords[2].Y, -trg.Coords[2].Z);
                            GL.Vertex3(trg.Coords[2].X, trg.Coords[2].Y, trg.Coords[2].Z);
                            GL.Vertex3(-trg.Coords[2].X, trg.Coords[2].Y, trg.Coords[2].Z);
                            GL.Vertex3(-trg.Coords[2].X, trg.Coords[2].Y, -trg.Coords[2].Z);
                            GL.Vertex3(-trg.Coords[2].X, -trg.Coords[2].Y, -trg.Coords[2].Z);
                            GL.Vertex3(trg.Coords[2].X, -trg.Coords[2].Y, -trg.Coords[2].Z);
                            GL.Vertex3(trg.Coords[2].X, -trg.Coords[2].Y, trg.Coords[2].Z);
                            GL.Vertex3(-trg.Coords[2].X, -trg.Coords[2].Y, trg.Coords[2].Z);
                            GL.Vertex3(-trg.Coords[2].X, -trg.Coords[2].Y, -trg.Coords[2].Z);
                            GL.End();
                            GL.Begin(PrimitiveType.Lines);
                            GL.Vertex3(-trg.Coords[2].X, -trg.Coords[2].Y, trg.Coords[2].Z);
                            GL.Vertex3(-trg.Coords[2].X, trg.Coords[2].Y, trg.Coords[2].Z);
                            GL.Vertex3(trg.Coords[2].X, -trg.Coords[2].Y, -trg.Coords[2].Z);
                            GL.Vertex3(trg.Coords[2].X, trg.Coords[2].Y, -trg.Coords[2].Z);
                            GL.Vertex3(trg.Coords[2].X, -trg.Coords[2].Y, trg.Coords[2].Z);
                            GL.Vertex3(trg.Coords[2].X, trg.Coords[2].Y, trg.Coords[2].Z);
                            GL.End();
                            
                            GL.PopMatrix();
                            GL.LineWidth(2);
                            GL.Begin(PrimitiveType.Lines);
                            foreach (var id in trg.Instances)
                            {
                                Instance inst = file.GetInstance(trg.Parent.Parent.ID, id);
                                GL.Vertex3(-trg.Coords[1].X, trg.Coords[1].Y, trg.Coords[1].Z);
                                GL.Vertex3(-inst.Pos.X, inst.Pos.Y, inst.Pos.Z);
                            }
                            GL.End();
                            GL.LineWidth(1);
                            DrawAxes(-trg.Coords[1].X, trg.Coords[1].Y, trg.Coords[1].Z, Math.Min(trg.Coords[2].X, Math.Min(trg.Coords[2].Y, trg.Coords[2].Z)) / 2);
                            Matrix3 rot_mat = Matrix3.CreateFromQuaternion(quat);
                            RenderString3D(trg.ID.ToString(), cur_color, -trg.Coords[1].X, trg.Coords[1].Y, trg.Coords[1].Z, ref rot_mat);
                        }
                    }

                    if (show_cams && file.Data.GetItem<TwinsSection>(i).ContainsItem(8))
                    {
                        if (file.Data.Type != TwinsFile.FileType.DemoRM2)
                        {
                            foreach (Camera cam in file.Data.GetItem<TwinsSection>(i).GetItem<TwinsSection>(8).Records)
                            {
                                GL.PushMatrix();
                                GL.Translate(-cam.TriggerPos.X, cam.TriggerPos.Y, cam.TriggerPos.Z);
                                Quaternion quat = new Quaternion(cam.TriggerRot.X, -cam.TriggerRot.Y, -cam.TriggerRot.Z, cam.TriggerRot.W);
                                Matrix4 new_mat = Matrix4.CreateFromQuaternion(quat);
                                GL.MultMatrix(ref new_mat);

                                cur_color = file.SelectedItem == cam ? Color.White : colors[colors.Length - i * 2 - 2];
                                GL.DepthMask(false);
                                GL.Enable(EnableCap.Lighting);
                                GL.Color4(cur_color.R, cur_color.G, cur_color.B, (byte)95);
                                GL.Begin(PrimitiveType.QuadStrip);
                                GL.Vertex3(-cam.TriggerSize.X, -cam.TriggerSize.Y, -cam.TriggerSize.Z);
                                GL.Vertex3(cam.TriggerSize.X, -cam.TriggerSize.Y, -cam.TriggerSize.Z);
                                GL.Vertex3(-cam.TriggerSize.X, cam.TriggerSize.Y, -cam.TriggerSize.Z);
                                GL.Vertex3(cam.TriggerSize.X, cam.TriggerSize.Y, -cam.TriggerSize.Z);
                                GL.Vertex3(-cam.TriggerSize.X, cam.TriggerSize.Y, cam.TriggerSize.Z);
                                GL.Vertex3(cam.TriggerSize.X, cam.TriggerSize.Y, cam.TriggerSize.Z);
                                GL.Vertex3(-cam.TriggerSize.X, -cam.TriggerSize.Y, cam.TriggerSize.Z);
                                GL.Vertex3(cam.TriggerSize.X, -cam.TriggerSize.Y, cam.TriggerSize.Z);
                                GL.Vertex3(-cam.TriggerSize.X, -cam.TriggerSize.Y, -cam.TriggerSize.Z);
                                GL.Vertex3(cam.TriggerSize.X, -cam.TriggerSize.Y, -cam.TriggerSize.Z);
                                GL.End();
                                GL.Begin(PrimitiveType.Quads);

                                GL.Vertex3(-cam.TriggerSize.X, -cam.TriggerSize.Y, -cam.TriggerSize.Z);
                                GL.Vertex3(-cam.TriggerSize.X, -cam.TriggerSize.Y, cam.TriggerSize.Z);
                                GL.Vertex3(-cam.TriggerSize.X, cam.TriggerSize.Y, cam.TriggerSize.Z);
                                GL.Vertex3(-cam.TriggerSize.X, cam.TriggerSize.Y, -cam.TriggerSize.Z);

                                GL.Vertex3(cam.TriggerSize.X, -cam.TriggerSize.Y, -cam.TriggerSize.Z);
                                GL.Vertex3(cam.TriggerSize.X, -cam.TriggerSize.Y, cam.TriggerSize.Z);
                                GL.Vertex3(cam.TriggerSize.X, cam.TriggerSize.Y, cam.TriggerSize.Z);
                                GL.Vertex3(cam.TriggerSize.X, cam.TriggerSize.Y, -cam.TriggerSize.Z);

                                GL.End();
                                GL.DepthMask(true);
                                GL.Disable(EnableCap.Lighting);

                                GL.Color4(cur_color);
                                GL.LineWidth(1);

                                GL.Begin(PrimitiveType.LineStrip);
                                GL.Vertex3(-cam.TriggerSize.X, cam.TriggerSize.Y, -cam.TriggerSize.Z);
                                GL.Vertex3(cam.TriggerSize.X, cam.TriggerSize.Y, -cam.TriggerSize.Z);
                                GL.Vertex3(cam.TriggerSize.X, cam.TriggerSize.Y, cam.TriggerSize.Z);
                                GL.Vertex3(-cam.TriggerSize.X, cam.TriggerSize.Y, cam.TriggerSize.Z);
                                GL.Vertex3(-cam.TriggerSize.X, cam.TriggerSize.Y, -cam.TriggerSize.Z);
                                GL.Vertex3(-cam.TriggerSize.X, -cam.TriggerSize.Y, -cam.TriggerSize.Z);
                                GL.Vertex3(cam.TriggerSize.X, -cam.TriggerSize.Y, -cam.TriggerSize.Z);
                                GL.Vertex3(cam.TriggerSize.X, -cam.TriggerSize.Y, cam.TriggerSize.Z);
                                GL.Vertex3(-cam.TriggerSize.X, -cam.TriggerSize.Y, cam.TriggerSize.Z);
                                GL.Vertex3(-cam.TriggerSize.X, -cam.TriggerSize.Y, -cam.TriggerSize.Z);
                                GL.End();
                                GL.Begin(PrimitiveType.Lines);
                                GL.Vertex3(-cam.TriggerSize.X, -cam.TriggerSize.Y, cam.TriggerSize.Z);
                                GL.Vertex3(-cam.TriggerSize.X, cam.TriggerSize.Y, cam.TriggerSize.Z);
                                GL.Vertex3(cam.TriggerSize.X, -cam.TriggerSize.Y, -cam.TriggerSize.Z);
                                GL.Vertex3(cam.TriggerSize.X, cam.TriggerSize.Y, -cam.TriggerSize.Z);
                                GL.Vertex3(cam.TriggerSize.X, -cam.TriggerSize.Y, cam.TriggerSize.Z);
                                GL.Vertex3(cam.TriggerSize.X, cam.TriggerSize.Y, cam.TriggerSize.Z);
                                GL.End();

                                GL.PopMatrix();
                                DrawAxes(-cam.TriggerPos.X, cam.TriggerPos.Y, cam.TriggerPos.Z, Math.Min(cam.TriggerSize.X, Math.Min(cam.TriggerSize.Y, cam.TriggerSize.Z)) / 2);
                                Matrix3 rot_mat = Matrix3.CreateFromQuaternion(quat);
                                RenderString3D(cam.ID.ToString(), cur_color, -cam.TriggerPos.X, cam.TriggerPos.Y, cam.TriggerPos.Z, ref rot_mat);
                            }
                        }
                    }
                }
            }

            if (show_pos)
            {
                if (file.Data.GetItem<ParticleData>(8).Size > 12) //particle positions
                {
                    ParticleData partData = file.Data.GetItem<ParticleData>(8);
                    if (partData.ParticleInstanceCount > 0)
                    {
                        for (int p = 0; p < partData.ParticleInstanceCount; p++)
                        {
                            Matrix3 rot_ins = Matrix3.Identity;
                            rot_ins *= Matrix3.CreateRotationX(partData.ParticleInstances[p].Rot_X / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                            rot_ins *= Matrix3.CreateRotationY(-partData.ParticleInstances[p].Rot_Y / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                            rot_ins *= Matrix3.CreateRotationZ(-partData.ParticleInstances[p].Rot_Z / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                            GL.PointSize(5);
                            GL.Color3(Color.Pink);
                            GL.Begin(PrimitiveType.Points);
                            GL.Vertex3(-partData.ParticleInstances[p].Position.X, partData.ParticleInstances[p].Position.Y, partData.ParticleInstances[p].Position.Z);
                            GL.End();
                            RenderString3D(p.ToString(), Color.Pink, -partData.ParticleInstances[p].Position.X, partData.ParticleInstances[p].Position.Y, partData.ParticleInstances[p].Position.Z, ref rot_ins, 0.5F);
                        }
                    }
                }
                if (file.DataAux != null) // lights
                {
                    SceneryData scenData = file.DataAux.GetItem<SceneryData>(0);
                    if (scenData.LightsAmbient.Count > 0)
                    {
                        for (int p = 0; p < scenData.LightsAmbient.Count; p++)
                        {
                            Matrix3 rot_ins = Matrix3.Identity;
                            GL.PointSize(5);
                            GL.Color3(Color.White);
                            GL.Begin(PrimitiveType.Points);
                            GL.Vertex3(-scenData.LightsAmbient[p].Position.X, scenData.LightsAmbient[p].Position.Y, scenData.LightsAmbient[p].Position.Z);
                            GL.End();
                            RenderString3D(p.ToString(), Color.White, -scenData.LightsAmbient[p].Position.X, scenData.LightsAmbient[p].Position.Y, scenData.LightsAmbient[p].Position.Z, ref rot_ins, 0.5F);
                        }
                    }
                    if (scenData.LightsDirectional.Count > 0)
                    {
                        for (int p = 0; p < scenData.LightsDirectional.Count; p++)
                        {
                            Matrix3 rot_ins = Matrix3.Identity;
                            GL.PointSize(5);
                            GL.Color3(Color.LightYellow);
                            GL.Begin(PrimitiveType.Points);
                            GL.Vertex3(-scenData.LightsDirectional[p].Position.X, scenData.LightsDirectional[p].Position.Y, scenData.LightsDirectional[p].Position.Z);
                            GL.End();
                            RenderString3D(p.ToString(), Color.LightYellow, -scenData.LightsDirectional[p].Position.X, scenData.LightsDirectional[p].Position.Y, scenData.LightsDirectional[p].Position.Z, ref rot_ins, 0.5F);
                        }
                    }
                    if (scenData.LightsPoint.Count > 0)
                    {
                        for (int p = 0; p < scenData.LightsPoint.Count; p++)
                        {
                            Matrix3 rot_ins = Matrix3.Identity;
                            GL.PointSize(5);
                            GL.Color3(Color.Yellow);
                            GL.Begin(PrimitiveType.Points);
                            GL.Vertex3(-scenData.LightsPoint[p].Position.X, scenData.LightsPoint[p].Position.Y, scenData.LightsPoint[p].Position.Z);
                            GL.End();
                            RenderString3D(p.ToString(), Color.Yellow, -scenData.LightsPoint[p].Position.X, scenData.LightsPoint[p].Position.Y, scenData.LightsPoint[p].Position.Z, ref rot_ins, 0.5F);
                        }
                    }
                    if (scenData.LightsNegative.Count > 0)
                    {
                        for (int p = 0; p < scenData.LightsNegative.Count; p++)
                        {
                            Matrix3 rot_ins = Matrix3.Identity;
                            GL.PointSize(5);
                            GL.Color3(Color.LightGoldenrodYellow);
                            GL.Begin(PrimitiveType.Points);
                            GL.Vertex3(-scenData.LightsNegative[p].Position.X, scenData.LightsNegative[p].Position.Y, scenData.LightsNegative[p].Position.Z);
                            GL.End();
                            RenderString3D(p.ToString(), Color.LightYellow, -scenData.LightsNegative[p].Position.X, scenData.LightsNegative[p].Position.Y, scenData.LightsNegative[p].Position.Z, ref rot_ins, 0.5F);
                        }
                    }
                }
            }

            //Draw chunk links if available
            if (sm2_links && links != null)
            {
                GL.LineWidth(2);
                GL.DepthMask(false);
                foreach (var l in links.Links)
                {
                    Color cur_color = colors[(links.Links.IndexOf(l) + 2) % colors.Length];
                    GL.PushMatrix();
                    GL.Scale(-1, 1, 1);
                    if (l.HasWall())
                    {
                        GL.Color4(Color.FromArgb(95, cur_color));
                        GL.Begin(PrimitiveType.Quads);
                        GL.Vertex4(l.LoadWall[0].ToArray());
                        GL.Vertex4(l.LoadWall[1].ToArray());
                        GL.Vertex4(l.LoadWall[2].ToArray());
                        GL.Vertex4(l.LoadWall[3].ToArray());
                        GL.End();
                        GL.Color4(cur_color);
                        GL.Begin(PrimitiveType.LineLoop);
                        GL.Vertex4(l.LoadWall[0].ToArray());
                        GL.Vertex4(l.LoadWall[1].ToArray());
                        GL.Vertex4(l.LoadWall[2].ToArray());
                        GL.Vertex4(l.LoadWall[3].ToArray());
                        GL.End();
                        GL.Begin(PrimitiveType.Lines);
                        GL.Vertex4(l.LoadWall[0].ToArray());
                        GL.Vertex4(l.LoadWall[2].ToArray());
                        GL.Vertex4(l.LoadWall[1].ToArray());
                        GL.Vertex4(l.LoadWall[3].ToArray());
                        GL.End();
                        Matrix3 rot_mat = Matrix3.Identity;
                        rot_mat *= Matrix3.CreateRotationX(-rot.Y / 180 * MathHelper.Pi);
                        rot_mat *= Matrix3.CreateRotationY(-rot.X / 180 * MathHelper.Pi);
                        rot_mat *= Matrix3.CreateRotationZ(rot.Z / 180 * MathHelper.Pi);
                        RenderString3D(new string(l.Path), cur_color,
                            -(l.LoadWall[0].X + l.LoadWall[1].X + l.LoadWall[2].X + l.LoadWall[3].X) / 4,
                            (l.LoadWall[0].Y + l.LoadWall[1].Y + l.LoadWall[2].Y + l.LoadWall[3].Y) / 4,
                            (l.LoadWall[0].Z + l.LoadWall[1].Z + l.LoadWall[2].Z + l.LoadWall[3].Z) / 4,
                            ref rot_mat);
                    }
                    if (l.Type == 1 || l.Type == 3)
                    {
                        GL.Begin(PrimitiveType.Lines);
                        for (int i = 0; i < 6; ++i)
                        {
                            switch (i)
                            {
                                case 0: GL.Color4(Color.Red); break;
                                case 1: GL.Color4(Color.Green); break;
                                case 2: GL.Color4(Color.Blue); break;
                                case 3: GL.Color4(Color.Yellow); break;
                                case 4: GL.Color4(Color.Magenta); break;
                                case 5: GL.Color4(Color.Cyan); break;
                            }
                            int i1 = i >= 4 ? 1 - (i - 4) : (0 + 2 * i) % 8;
                            int i2 = i >= 4 ? i1 + 2 : (1 + 2 * i) % 8;
                            int i3 = i >= 4 ? i2 + 2 : (2 + 2 * i) % 8;
                            int i4 = i >= 4 ? i3 + 2 : (3 + 2 * i) % 8;
                            Vector3 mid_vec = new Vector3(l.LoadArea[i1].X + l.LoadArea[i2].X + l.LoadArea[i3].X + l.LoadArea[i4].X,
                                l.LoadArea[i1].Y + l.LoadArea[i2].Y + l.LoadArea[i3].Y + l.LoadArea[i4].Y,
                                l.LoadArea[i1].Z + l.LoadArea[i2].Z + l.LoadArea[i3].Z + l.LoadArea[i4].Z) / 4;
                            Vector3 nor_vec = new Vector3(l.AreaMatrix[i].X, l.AreaMatrix[i].Y, l.AreaMatrix[i].Z);
                            Vector3 unk_vec = new Vector3(l.UnknownMatrix[i].X, l.UnknownMatrix[i].Y, l.UnknownMatrix[i].Z);
                            GL.Vertex3(mid_vec);
                            GL.Vertex3(mid_vec + nor_vec);
                            GL.Vertex3(mid_vec);
                            GL.Vertex3(mid_vec + unk_vec);
                        }
                        GL.End();
                        GL.Enable(EnableCap.Lighting);
                        GL.Color4(Color.FromArgb(95, cur_color));
                        GL.Begin(PrimitiveType.QuadStrip);
                        GL.Vertex4(l.LoadArea[0].ToArray());
                        GL.Vertex4(l.LoadArea[1].ToArray());
                        GL.Vertex4(l.LoadArea[2].ToArray());
                        GL.Vertex4(l.LoadArea[3].ToArray());
                        GL.Vertex4(l.LoadArea[4].ToArray());
                        GL.Vertex4(l.LoadArea[5].ToArray());
                        GL.Vertex4(l.LoadArea[6].ToArray());
                        GL.Vertex4(l.LoadArea[7].ToArray());
                        GL.Vertex4(l.LoadArea[0].ToArray());
                        GL.Vertex4(l.LoadArea[1].ToArray());
                        GL.End();
                        GL.Begin(PrimitiveType.Quads);
                        GL.Vertex4(l.LoadArea[1].ToArray());
                        GL.Vertex4(l.LoadArea[3].ToArray());
                        GL.Vertex4(l.LoadArea[5].ToArray());
                        GL.Vertex4(l.LoadArea[7].ToArray());
                        GL.Vertex4(l.LoadArea[0].ToArray());
                        GL.Vertex4(l.LoadArea[2].ToArray());
                        GL.Vertex4(l.LoadArea[4].ToArray());
                        GL.Vertex4(l.LoadArea[6].ToArray());
                        GL.End();
                        GL.Disable(EnableCap.Lighting);
                        GL.Color4(cur_color);
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                        GL.Begin(PrimitiveType.QuadStrip);
                        GL.Vertex4(l.LoadArea[0].ToArray());
                        GL.Vertex4(l.LoadArea[1].ToArray());
                        GL.Vertex4(l.LoadArea[2].ToArray());
                        GL.Vertex4(l.LoadArea[3].ToArray());
                        GL.Vertex4(l.LoadArea[4].ToArray());
                        GL.Vertex4(l.LoadArea[5].ToArray());
                        GL.Vertex4(l.LoadArea[6].ToArray());
                        GL.Vertex4(l.LoadArea[7].ToArray());
                        GL.Vertex4(l.LoadArea[0].ToArray());
                        GL.Vertex4(l.LoadArea[1].ToArray());
                        GL.End();
                        GL.Begin(PrimitiveType.Quads);
                        GL.Vertex4(l.LoadArea[1].ToArray());
                        GL.Vertex4(l.LoadArea[3].ToArray());
                        GL.Vertex4(l.LoadArea[5].ToArray());
                        GL.Vertex4(l.LoadArea[7].ToArray());
                        GL.Vertex4(l.LoadArea[0].ToArray());
                        GL.Vertex4(l.LoadArea[2].ToArray());
                        GL.Vertex4(l.LoadArea[4].ToArray());
                        GL.Vertex4(l.LoadArea[6].ToArray());
                        GL.End();
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                        GL.Begin(PrimitiveType.Lines);
                        GL.Vertex4(l.LoadArea[0].ToArray());
                        GL.Vertex4(l.LoadArea[3].ToArray());
                        GL.Vertex4(l.LoadArea[1].ToArray());
                        GL.Vertex4(l.LoadArea[2].ToArray());
                        GL.Vertex4(l.LoadArea[2].ToArray());
                        GL.Vertex4(l.LoadArea[5].ToArray());
                        GL.Vertex4(l.LoadArea[3].ToArray());
                        GL.Vertex4(l.LoadArea[4].ToArray());
                        GL.Vertex4(l.LoadArea[4].ToArray());
                        GL.Vertex4(l.LoadArea[7].ToArray());
                        GL.Vertex4(l.LoadArea[5].ToArray());
                        GL.Vertex4(l.LoadArea[6].ToArray());
                        GL.Vertex4(l.LoadArea[6].ToArray());
                        GL.Vertex4(l.LoadArea[1].ToArray());
                        GL.Vertex4(l.LoadArea[7].ToArray());
                        GL.Vertex4(l.LoadArea[0].ToArray());
                        GL.Vertex4(l.LoadArea[0].ToArray());
                        GL.Vertex4(l.LoadArea[4].ToArray());
                        GL.Vertex4(l.LoadArea[2].ToArray());
                        GL.Vertex4(l.LoadArea[6].ToArray());
                        GL.Vertex4(l.LoadArea[1].ToArray());
                        GL.Vertex4(l.LoadArea[5].ToArray());
                        GL.Vertex4(l.LoadArea[3].ToArray());
                        GL.Vertex4(l.LoadArea[7].ToArray());
                        GL.End();
                    }
                    GL.PopMatrix();
                }
                GL.DepthMask(true);
                GL.LineWidth(1);
            }

            GL.PopMatrix();

            GL.LineWidth(1);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.C:
                case Keys.L:
                case Keys.T:
                case Keys.X:
                case Keys.Y:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.Oemtilde:
                    show_scenery = !show_scenery;
                    break;
                case Keys.D1:
                    show_col = !show_col;
                    break;
                case Keys.D2:
                    show_hurtzones = !show_hurtzones;
                    break;
                case Keys.D3:
                    show_invis = !show_invis;
                    break;
                case Keys.D4:
                    obj_models = !obj_models;
                    break;
                case Keys.D5:
                    sm2_links = !sm2_links;
                    break;
                case Keys.D6:
                    show_triggers = !show_triggers;
                    break;
                case Keys.D7:
                    show_cams = !show_cams;
                    break;
                case Keys.D8:
                    show_inst = !show_inst;
                    break;
                case Keys.D9:
                    show_pos = !show_pos;
                    break;
                case Keys.D0:
                    show_skydome = !show_skydome;
                    break;
                case Keys.OemMinus:
                    show_linked_chunks = !show_linked_chunks;
                    break;
                case Keys.Oemplus:
                    show_hud = !show_hud;
                    break;
                case Keys.OemOpenBrackets:
                    wire_col = !wire_col;
                    break;
                case Keys.OemCloseBrackets:
                    show_col_nodes = !show_col_nodes;
                    break;
            }
        }

        public void LoadColTree(FileController fcont, int layer, bool islinked, Matrix4 ChunkMatrix, int LinkID)
        {
            ColData data = fcont.Data.GetItem<ColData>(9);
            List<Vertex> vertices = new List<Vertex>(data.Vertices.Count);
            vtx[reserved_layers + static_layers] = new VertexBufferData();
            vtx[reserved_layers + static_layers].VtxInd = new uint[data.Tris.Count * 3];
            if (islinked)
            {
                vtx[reserved_layers + static_layers].Type = VertexBufferData.BufferType.ExtraCollision;
                vtx[reserved_layers + static_layers].LinkID = LinkID;
            }
            else
            {
                vtx[reserved_layers + static_layers].Type = VertexBufferData.BufferType.Collision;
            }
            vtx[reserved_layers + static_layers].LayerID = layer;

            for (int i = 0; i < data.Vertices.Count; ++i)
            {
                Vector4 v = data.Vertices[i].ToVec4();
                v.X = -v.X;

                if (islinked)
                {
                    v *= ChunkMatrix;
                }

                Vector3 v_3 = new Vector3(v.X,v.Y,v.Z);

                vertices.Add(new Vertex(v_3));
            }
            for (int i = 0; i < data.Tris.Count; ++i)
            {
                if (data.Tris[i].Surface == layer)
                {
                    uint col = Vertex.ColorToABGR(colors[data.Tris[i].Surface % colors.Length]);
                    int v1 = data.Tris[i].Vert1;
                    if (vertices[v1].Col != 0 && vertices[v1].Col != col)
                    {
                        vertices.Add(vertices[v1]);
                        v1 = vertices.Count - 1;
                    }
                    int v2 = data.Tris[i].Vert2;
                    if (vertices[v2].Col != 0 && vertices[v2].Col != col)
                    {
                        vertices.Add(vertices[v2]);
                        v2 = vertices.Count - 1;
                    }
                    int v3 = data.Tris[i].Vert3;
                    if (vertices[v3].Col != 0 && vertices[v3].Col != col)
                    {
                        vertices.Add(vertices[v3]);
                        v3 = vertices.Count - 1;
                    }
                    vtx[reserved_layers + static_layers].VtxInd[i * 3 + 0] = (uint)v1;
                    vtx[reserved_layers + static_layers].VtxInd[i * 3 + 1] = (uint)v2;
                    vtx[reserved_layers + static_layers].VtxInd[i * 3 + 2] = (uint)v3;
                    Vector3 normal = VectorFuncs.CalcNormal(vertices[v1].Pos, vertices[v2].Pos, vertices[v3].Pos);
                    var v = vertices[v1];
                    v.Nor += normal;
                    v.Col = col;
                    vertices[v1] = v;
                    v = vertices[v2];
                    v.Nor += normal;
                    v.Col = col;
                    vertices[v2] = v;
                    v = vertices[v3];
                    v.Nor += normal;
                    v.Col = col;
                    vertices[v3] = v;
                }
            }
            vtx[reserved_layers + static_layers].Vtx = vertices.ToArray();
            UpdateVBO(reserved_layers + static_layers);
            static_layers++;
        }

        public void LoadInstances()
        {
            float min_x = float.MaxValue, min_y = float.MaxValue, min_z = float.MaxValue, max_x = float.MinValue, max_y = float.MinValue, max_z = float.MinValue;
            bool[] record_exists = new bool[8];
            int inst_count = 0;
            for (uint i = 0; i <= 7; ++i)
            {
                record_exists[i] = file.Data.ContainsItem(i);
                if (record_exists[i])
                {
                    if (file.Data.GetItem<TwinsSection>(i).ContainsItem(6))
                        inst_count += file.Data.GetItem<TwinsSection>(i).GetItem<TwinsSection>(6).Records.Count;
                    else record_exists[i] = false;
                }
            }
            if (vtx[1].Vtx == null || vtx[1].Vtx.Length != 22 * inst_count)
            {
                vtx[1].VtxCounts = new int[7 * inst_count];
                vtx[1].VtxOffs = new int[7 * inst_count];
                vtx[1].Vtx = new Vertex[22 * inst_count];
                for (int i = 0; i < inst_count; ++i)
                {
                    vtx[1].VtxCounts[i * 7 + 0] = 2;
                    vtx[1].VtxCounts[i * 7 + 1] = 2;
                    vtx[1].VtxCounts[i * 7 + 2] = 2;
                    vtx[1].VtxCounts[i * 7 + 3] = 8;
                    vtx[1].VtxCounts[i * 7 + 4] = 4;
                    vtx[1].VtxCounts[i * 7 + 5] = 2;
                    vtx[1].VtxCounts[i * 7 + 6] = 2;
                }
            }
            int l = 0, m = 0, cur_instance = 0;
            for (uint i = 0; i <= 7; ++i)
            {
                if (!record_exists[i]) continue;
                if (file.Data.GetItem<TwinsSection>(i).ContainsItem(6))
                {
                    if (file.Data.Type != TwinsFile.FileType.DemoRM2)
                    {
                        foreach (Instance ins in file.Data.GetItem<TwinsSection>(i).GetItem<TwinsSection>(6).Records)
                        {
                            Matrix3 rot_ins = Matrix3.Identity;
                            rot_ins *= Matrix3.CreateRotationX(ins.RotX / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                            rot_ins *= Matrix3.CreateRotationY(-ins.RotY / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                            rot_ins *= Matrix3.CreateRotationZ(-ins.RotZ / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                            Vector3 pos_ins = ins.Pos.ToVec3();
                            pos_ins.X = -pos_ins.X;
                            vtx[1].VtxOffs[l++] = m;
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(-indicator_size * 0.75f, 0, 0) + pos_ins, Color.Red);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(+indicator_size * 0.375f, 0, 0) + pos_ins, Color.Red);
                            vtx[1].VtxOffs[l++] = m;
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(0, indicator_size * 0.75f, 0) + pos_ins, Color.Green);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(0, -indicator_size * 0.375f, 0) + pos_ins, Color.Green);
                            vtx[1].VtxOffs[l++] = m;
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(0, 0, indicator_size * 0.75f) + pos_ins, Color.Blue);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(0, 0, -indicator_size * 0.375f) + pos_ins, Color.Blue);
                            vtx[1].VtxOffs[l++] = m;
                            Color cur_color = (file.SelectedItem == ins) ? Color.White : colors[colors.Length - i * 2 - 1];
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(-indicator_size, -indicator_size + 0.5f, -indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(+indicator_size, -indicator_size + 0.5f, -indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(+indicator_size, +indicator_size + 0.5f, -indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(-indicator_size, +indicator_size + 0.5f, -indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(-indicator_size, -indicator_size + 0.5f, -indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(-indicator_size, -indicator_size + 0.5f, +indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(+indicator_size, -indicator_size + 0.5f, +indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(+indicator_size, -indicator_size + 0.5f, -indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].VtxOffs[l++] = m;
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(-indicator_size, -indicator_size + 0.5f, +indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(-indicator_size, +indicator_size + 0.5f, +indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(+indicator_size, +indicator_size + 0.5f, +indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(+indicator_size, -indicator_size + 0.5f, +indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].VtxOffs[l++] = m;
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(-indicator_size, +indicator_size + 0.5f, +indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(-indicator_size, +indicator_size + 0.5f, -indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].VtxOffs[l++] = m;
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(+indicator_size, +indicator_size + 0.5f, +indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(+indicator_size, +indicator_size + 0.5f, -indicator_size) * rot_ins + pos_ins, cur_color);
                            min_x = Math.Min(min_x, pos_ins.X);
                            min_y = Math.Min(min_y, pos_ins.Y);
                            min_z = Math.Min(min_z, pos_ins.Z);
                            max_x = Math.Max(max_x, pos_ins.X);
                            max_y = Math.Max(max_y, pos_ins.Y);
                            max_z = Math.Max(max_z, pos_ins.Z);

                            if (file.Data.Type == TwinsFile.FileType.RM2)
                            {
                                MeshController modelCont = null;
                                bool HasMesh = false;
                                ushort TargetGI = 65535;
                                List<uint> ModelList = new List<uint>();
                                List<Matrix4> LocalRotList = new List<Matrix4>();
                                uint TargetModel = 65535;
                                Matrix4 LocalRot = Matrix4.Identity;
                                FileController targetFile = file;
                                Vector4 pos_ins_4 = ins.Pos.ToVec4();
                                pos_ins_4.X = -pos_ins_4.X;
                                Matrix4 rot_ins_4 = Matrix4.Identity;
                                rot_ins_4 *= Matrix4.CreateRotationX(ins.RotX / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                                rot_ins_4 *= Matrix4.CreateRotationY(-ins.RotY / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                                rot_ins_4 *= Matrix4.CreateRotationZ(-ins.RotZ / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);

                                foreach (GameObject gameObject in targetFile.Data.GetItem<TwinsSection>(10).GetItem<TwinsSection>(0).Records)
                                {
                                    if (gameObject.ID == ins.ObjectID)
                                    {
                                        if (gameObject.OGIs.Count > 0 && gameObject.OGIs[0] != 65535)
                                        {
                                            if (ins.UnkI323[0] != 0)
                                            {
                                                if (gameObject.OGIs.Count > ins.UnkI323[0] && gameObject.OGIs[(int)ins.UnkI323[0]] != 65535)
                                                {
                                                    TargetGI = gameObject.OGIs[(int)ins.UnkI323[0]];
                                                }
                                                else
                                                {
                                                    TargetGI = gameObject.OGIs[0];
                                                }
                                            }
                                            else
                                            {
                                                TargetGI = gameObject.OGIs[0];
                                            }
                                        }
                                    }
                                }

                                if (TargetGI == 65535)
                                {
                                    if (file.DataDefault != null)
                                    {
                                        targetFile = file.DefaultCont;
                                        foreach (GameObject gameObject in file.DataDefault.GetItem<TwinsSection>(10).GetItem<TwinsSection>(0).Records)
                                        {
                                            if (gameObject.ID == ins.ObjectID)
                                            {
                                                if (gameObject.OGIs.Count > 0 && gameObject.OGIs[0] != 65535)
                                                {
                                                    TargetGI = gameObject.OGIs[0];
                                                }
                                            }
                                        }
                                    }
                                }
                                
                                if (TargetGI != 65535)
                                {
                                    foreach (GraphicsInfo GI in targetFile.Data.GetItem<TwinsSection>(10).GetItem<TwinsSection>(3).Records)
                                    {
                                        if (GI.ID == TargetGI)
                                        {
                                            if (GI.ModelIDs.Length > 0)
                                            {
                                                for (int gi_model = 0; gi_model < GI.ModelIDs.Length; gi_model++)
                                                {
                                                    if (GI.ModelIDs[gi_model].ModelID != 65535)
                                                    {
                                                        ModelList.Add(GI.ModelIDs[gi_model].ModelID);
                                                        Matrix4 tempRot = Matrix4.Identity;

                                                        // Rotation
                                                        tempRot.M11 = -GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[0].X;
                                                        tempRot.M12 = -GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[1].X;
                                                        tempRot.M13 = -GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[2].X;
                                                        
                                                        tempRot.M21 = GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[0].Y;
                                                        tempRot.M22 = GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[1].Y;
                                                        tempRot.M23 = GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[2].Y;
                                                        
                                                        tempRot.M31 = GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[0].Z;
                                                        tempRot.M32 = GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[1].Z;
                                                        tempRot.M33 = GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[2].Z;

                                                        tempRot.M14 = GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[0].W;
                                                        tempRot.M24 = GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[1].W;
                                                        tempRot.M34 = GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[2].W;

                                                        // Position
                                                        tempRot.M41 = GI.Type1[GI.ModelIDs[gi_model].ID].Matrix[1].X;
                                                        tempRot.M42 = GI.Type1[GI.ModelIDs[gi_model].ID].Matrix[1].Y;
                                                        tempRot.M43 = GI.Type1[GI.ModelIDs[gi_model].ID].Matrix[1].Z;
                                                        tempRot.M44 = GI.Type1[GI.ModelIDs[gi_model].ID].Matrix[1].W;

                                                        // Adjusted for OpenTK
                                                        tempRot *= Matrix4.CreateScale(-1, 1, 1);

                                                        LocalRotList.Add(tempRot);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (ModelList.Count > 0)
                                    {
                                        for (int modelID = 0; modelID < ModelList.Count; modelID++)
                                        {
                                            HasMesh = false;
                                            TargetModel = ModelList[modelID];
                                            LocalRot = LocalRotList[modelID];
                                            if (TargetModel != 65535)
                                            {
                                                SectionController mesh_sec = targetFile.GetItem<SectionController>(11).GetItem<SectionController>(2);
                                                foreach (Model model in targetFile.Data.GetItem<TwinsSection>(11).GetItem<TwinsSection>(3).Records)
                                                {
                                                    if (model.ID == TargetModel)
                                                    {
                                                        uint meshID = targetFile.Data.GetItem<TwinsSection>(11).GetItem<TwinsSection>(3).GetItem<Model>(TargetModel).MeshID;
                                                        modelCont = mesh_sec.GetItem<MeshController>(meshID);
                                                        HasMesh = true;
                                                    }
                                                }
                                            }

                                            if (HasMesh)
                                            {
                                                modelCont.LoadMeshData();
                                                Vertex[] vbuffer = new Vertex[modelCont.Vertices.Length];

                                                for (int v = 0; v < modelCont.Vertices.Length; v++)
                                                {
                                                    vbuffer[v] = modelCont.Vertices[v];
                                                    Vector4 targetPos = new Vector4(modelCont.Vertices[v].Pos.X, modelCont.Vertices[v].Pos.Y, modelCont.Vertices[v].Pos.Z, 1);

                                                    targetPos *= LocalRot;

                                                    bool rotationOverride = false;
                                                    if (ins.ObjectID == (ushort)DefaultEnums.ObjectID.ICICLE)
                                                    {
                                                        if (ins.UnkI323[0] == 1)
                                                        {
                                                            Matrix4 rot_ins_fix = Matrix4.Identity;
                                                            rot_ins_fix *= Matrix4.CreateRotationZ((32768) / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                                                            targetPos *= rot_ins_fix;
                                                            rotationOverride = true;
                                                        }
                                                    }

                                                    if (!rotationOverride)
                                                    {
                                                        targetPos *= rot_ins_4;
                                                    }
                                                    
                                                    targetPos += pos_ins_4;
                                                    modelCont.Vertices[v].Pos = new Vector3(targetPos.X, targetPos.Y, targetPos.Z);
                                                    if (ins.ObjectID == (ushort)DefaultEnums.ObjectID.TNTCRATE)
                                                    {
                                                        modelCont.Vertices[v].Col = Vertex.ColorToABGR(Color.Red);
                                                    }
                                                    else if (ins.ObjectID == (ushort)DefaultEnums.ObjectID.NITROCRATE)
                                                    {
                                                        modelCont.Vertices[v].Col = Vertex.ColorToABGR(Color.Green);
                                                    }
                                                    else if (WoodCrates.Contains((DefaultEnums.ObjectID)ins.ObjectID))
                                                    {
                                                        modelCont.Vertices[v].Col = Vertex.ColorToABGR(Color.SandyBrown);
                                                    }
                                                }
                                                vtx[reserved_layers + static_layers + cur_instance] = new VertexBufferData();
                                                vtx[reserved_layers + static_layers + cur_instance].Vtx = modelCont.Vertices;
                                                vtx[reserved_layers + static_layers + cur_instance].VtxInd = modelCont.Indices;
                                                vtx[reserved_layers + static_layers + cur_instance].Type = VertexBufferData.BufferType.Object;
                                                modelCont.Vertices = vbuffer;
                                                UpdateVBO(reserved_layers + static_layers + cur_instance);
                                            }
                                            else
                                            {
                                                vtx[reserved_layers + static_layers + cur_instance] = null;
                                            }
                                            cur_instance++;
                                        }
                                    }
                                }
                                
                            }
                        }
                    }
                    else
                    {
                        foreach (InstanceDemo ins in file.Data.GetItem<TwinsSection>(i).GetItem<TwinsSection>(6).Records)
                        {
                            Matrix3 rot_ins = Matrix3.Identity;
                            rot_ins *= Matrix3.CreateRotationX(ins.RotX / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                            rot_ins *= Matrix3.CreateRotationY(-ins.RotY / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                            rot_ins *= Matrix3.CreateRotationZ(-ins.RotZ / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                            Vector3 pos_ins = ins.Pos.ToVec3();
                            pos_ins.X = -pos_ins.X;
                            vtx[1].VtxOffs[l++] = m;
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(-indicator_size * 0.75f, 0, 0) + pos_ins, Color.Red);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(+indicator_size * 0.375f, 0, 0) + pos_ins, Color.Red);
                            vtx[1].VtxOffs[l++] = m;
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(0, indicator_size * 0.75f, 0) + pos_ins, Color.Green);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(0, -indicator_size * 0.375f, 0) + pos_ins, Color.Green);
                            vtx[1].VtxOffs[l++] = m;
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(0, 0, indicator_size * 0.75f) + pos_ins, Color.Blue);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(0, 0, -indicator_size * 0.375f) + pos_ins, Color.Blue);
                            vtx[1].VtxOffs[l++] = m;
                            Color cur_color = (file.SelectedItem == ins) ? Color.White : colors[colors.Length - i * 2 - 1];
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(-indicator_size, -indicator_size + 0.5f, -indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(+indicator_size, -indicator_size + 0.5f, -indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(+indicator_size, +indicator_size + 0.5f, -indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(-indicator_size, +indicator_size + 0.5f, -indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(-indicator_size, -indicator_size + 0.5f, -indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(-indicator_size, -indicator_size + 0.5f, +indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(+indicator_size, -indicator_size + 0.5f, +indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(+indicator_size, -indicator_size + 0.5f, -indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].VtxOffs[l++] = m;
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(-indicator_size, -indicator_size + 0.5f, +indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(-indicator_size, +indicator_size + 0.5f, +indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(+indicator_size, +indicator_size + 0.5f, +indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(+indicator_size, -indicator_size + 0.5f, +indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].VtxOffs[l++] = m;
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(-indicator_size, +indicator_size + 0.5f, +indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(-indicator_size, +indicator_size + 0.5f, -indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].VtxOffs[l++] = m;
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(+indicator_size, +indicator_size + 0.5f, +indicator_size) * rot_ins + pos_ins, cur_color);
                            vtx[1].Vtx[m++] = new Vertex(new Vector3(+indicator_size, +indicator_size + 0.5f, -indicator_size) * rot_ins + pos_ins, cur_color);
                            min_x = Math.Min(min_x, pos_ins.X);
                            min_y = Math.Min(min_y, pos_ins.Y);
                            min_z = Math.Min(min_z, pos_ins.Z);
                            max_x = Math.Max(max_x, pos_ins.X);
                            max_y = Math.Max(max_y, pos_ins.Y);
                            max_z = Math.Max(max_z, pos_ins.Z);

                            MeshController modelCont = null;
                            bool HasMesh = false;
                            ushort TargetGI = 65535;
                            List<uint> ModelList = new List<uint>();
                            List<Matrix4> LocalRotList = new List<Matrix4>();
                            uint TargetModel = 65535;
                            Matrix4 LocalRot = Matrix4.Identity;
                            FileController targetFile = file;
                            Vector4 pos_ins_4 = ins.Pos.ToVec4();
                            pos_ins_4.X = -pos_ins_4.X;
                            Matrix4 rot_ins_4 = Matrix4.Identity;
                            rot_ins_4 *= Matrix4.CreateRotationX(ins.RotX / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                            rot_ins_4 *= Matrix4.CreateRotationY(-ins.RotY / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                            rot_ins_4 *= Matrix4.CreateRotationZ(-ins.RotZ / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);

                            foreach (GameObjectDemo gameObject in targetFile.Data.GetItem<TwinsSection>(10).GetItem<TwinsSection>(0).Records)
                            {
                                if (gameObject.ID == ins.ObjectID)
                                {
                                    if (gameObject.OGIs.Count > 0 && gameObject.OGIs[0] != 65535)
                                    {
                                        if (ins.UnkI323[0] != 0)
                                        {
                                            if (gameObject.OGIs.Count > ins.UnkI323[0] && gameObject.OGIs[(int)ins.UnkI323[0]] != 65535)
                                            {
                                                TargetGI = gameObject.OGIs[(int)ins.UnkI323[0]];
                                            }
                                            else
                                            {
                                                TargetGI = gameObject.OGIs[0];
                                            }
                                        }
                                        else
                                        {
                                            TargetGI = gameObject.OGIs[0];
                                        }
                                    }
                                }
                            }

                            if (TargetGI == 65535)
                            {
                                if (file.DataDefault != null)
                                {
                                    targetFile = file.DefaultCont;
                                    foreach (GameObjectDemo gameObject in file.DataDefault.GetItem<TwinsSection>(10).GetItem<TwinsSection>(0).Records)
                                    {
                                        if (gameObject.ID == ins.ObjectID)
                                        {
                                            if (gameObject.OGIs.Count > 0 && gameObject.OGIs[0] != 65535)
                                            {
                                                TargetGI = gameObject.OGIs[0];
                                            }
                                        }
                                    }
                                }
                            }

                            if (TargetGI != 65535)
                            {
                                foreach (GraphicsInfo GI in targetFile.Data.GetItem<TwinsSection>(10).GetItem<TwinsSection>(3).Records)
                                {
                                    if (GI.ID == TargetGI)
                                    {
                                        if (GI.ModelIDs.Length > 0)
                                        {
                                            for (int gi_model = 0; gi_model < GI.ModelIDs.Length; gi_model++)
                                            {
                                                if (GI.ModelIDs[gi_model].ModelID != 65535)
                                                {
                                                    ModelList.Add(GI.ModelIDs[gi_model].ModelID);
                                                    Matrix4 tempRot = Matrix4.Identity;

                                                    // Rotation
                                                    tempRot.M11 = -GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[0].X;
                                                    tempRot.M12 = -GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[1].X;
                                                    tempRot.M13 = -GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[2].X;

                                                    tempRot.M21 = GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[0].Y;
                                                    tempRot.M22 = GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[1].Y;
                                                    tempRot.M23 = GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[2].Y;

                                                    tempRot.M31 = GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[0].Z;
                                                    tempRot.M32 = GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[1].Z;
                                                    tempRot.M33 = GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[2].Z;

                                                    tempRot.M14 = GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[0].W;
                                                    tempRot.M24 = GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[1].W;
                                                    tempRot.M34 = GI.Type3[GI.ModelIDs[gi_model].ID].Matrix[2].W;

                                                    // Position
                                                    tempRot.M41 = GI.Type1[GI.ModelIDs[gi_model].ID].Matrix[1].X;
                                                    tempRot.M42 = GI.Type1[GI.ModelIDs[gi_model].ID].Matrix[1].Y;
                                                    tempRot.M43 = GI.Type1[GI.ModelIDs[gi_model].ID].Matrix[1].Z;
                                                    tempRot.M44 = GI.Type1[GI.ModelIDs[gi_model].ID].Matrix[1].W;

                                                    // Adjusted for OpenTK
                                                    tempRot *= Matrix4.CreateScale(-1, 1, 1);

                                                    LocalRotList.Add(tempRot);
                                                }
                                            }
                                        }
                                    }
                                }

                                if (ModelList.Count > 0)
                                {
                                    for (int modelID = 0; modelID < ModelList.Count; modelID++)
                                    {
                                        HasMesh = false;
                                        TargetModel = ModelList[modelID];
                                        LocalRot = LocalRotList[modelID];
                                        if (TargetModel != 65535)
                                        {
                                            SectionController mesh_sec = targetFile.GetItem<SectionController>(11).GetItem<SectionController>(2);
                                            foreach (Model model in targetFile.Data.GetItem<TwinsSection>(11).GetItem<TwinsSection>(3).Records)
                                            {
                                                if (model.ID == TargetModel)
                                                {
                                                    uint meshID = targetFile.Data.GetItem<TwinsSection>(11).GetItem<TwinsSection>(3).GetItem<Model>(TargetModel).MeshID;
                                                    modelCont = mesh_sec.GetItem<MeshController>(meshID);
                                                    HasMesh = true;
                                                }
                                            }
                                        }

                                        if (HasMesh)
                                        {
                                            modelCont.LoadMeshData();
                                            Vertex[] vbuffer = new Vertex[modelCont.Vertices.Length];

                                            for (int v = 0; v < modelCont.Vertices.Length; v++)
                                            {
                                                vbuffer[v] = modelCont.Vertices[v];
                                                Vector4 targetPos = new Vector4(modelCont.Vertices[v].Pos.X, modelCont.Vertices[v].Pos.Y, modelCont.Vertices[v].Pos.Z, 1);

                                                targetPos *= LocalRot;

                                                bool rotationOverride = false;
                                                if (ins.ObjectID == (ushort)DefaultEnums.ObjectID.ICICLE)
                                                {
                                                    if (ins.UnkI323[0] == 1)
                                                    {
                                                        Matrix4 rot_ins_fix = Matrix4.Identity;
                                                        rot_ins_fix *= Matrix4.CreateRotationZ((32768) / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                                                        targetPos *= rot_ins_fix;
                                                        rotationOverride = true;
                                                    }
                                                }

                                                if (!rotationOverride)
                                                {
                                                    targetPos *= rot_ins_4;
                                                }

                                                targetPos += pos_ins_4;
                                                modelCont.Vertices[v].Pos = new Vector3(targetPos.X, targetPos.Y, targetPos.Z);
                                                if (ins.ObjectID == (ushort)DefaultEnums.ObjectID.TNTCRATE)
                                                {
                                                    modelCont.Vertices[v].Col = Vertex.ColorToABGR(Color.Red);
                                                }
                                                else if (ins.ObjectID == (ushort)DefaultEnums.ObjectID.NITROCRATE)
                                                {
                                                    modelCont.Vertices[v].Col = Vertex.ColorToABGR(Color.Green);
                                                }
                                                else if (WoodCrates.Contains((DefaultEnums.ObjectID)ins.ObjectID))
                                                {
                                                    modelCont.Vertices[v].Col = Vertex.ColorToABGR(Color.SandyBrown);
                                                }
                                            }
                                            vtx[reserved_layers + static_layers + cur_instance] = new VertexBufferData();
                                            vtx[reserved_layers + static_layers + cur_instance].Vtx = modelCont.Vertices;
                                            vtx[reserved_layers + static_layers + cur_instance].VtxInd = modelCont.Indices;
                                            vtx[reserved_layers + static_layers + cur_instance].Type = VertexBufferData.BufferType.Object;
                                            modelCont.Vertices = vbuffer;
                                            UpdateVBO(reserved_layers + static_layers + cur_instance);
                                        }
                                        else
                                        {
                                            vtx[reserved_layers + static_layers + cur_instance] = null;
                                        }
                                        cur_instance++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            zFar = Math.Max(zFar, Math.Max(max_x - min_x, Math.Max(max_y - min_y, max_z - min_z)));
            UpdateVBO(1);
        }

        public void LoadColNodes()
        {
            float min_x = float.MaxValue, min_y = float.MaxValue, min_z = float.MaxValue, max_x = float.MinValue, max_y = float.MinValue, max_z = float.MinValue;
            ColData data = file.Data.GetItem<ColData>(9);
            vtx[2].Vtx = new Vertex[data.Triggers.Count * 16];
            vtx[2].VtxCounts = new int[4 * data.Triggers.Count];
            vtx[2].VtxOffs = new int[4 * data.Triggers.Count];
            for (int i = 0; i < data.Triggers.Count; ++i)
            {
                vtx[2].VtxCounts[i * 4 + 0] = 8;
                vtx[2].VtxCounts[i * 4 + 1] = 4;
                vtx[2].VtxCounts[i * 4 + 2] = 2;
                vtx[2].VtxCounts[i * 4 + 3] = 2;
            }
            int l = 0, m = 0;
            foreach (var i in data.Triggers)
            {
                Color cur_color = (i.Flag1 == i.Flag2 && i.Flag1 < 0) ? Color.Cyan : Color.Red;
                vtx[2].VtxOffs[l++] = m;
                vtx[2].Vtx[m++] = new Vertex(new Vector3(-i.X1, i.Y1, i.Z1), cur_color);
                vtx[2].Vtx[m++] = new Vertex(new Vector3(-i.X2, i.Y1, i.Z1), cur_color);
                vtx[2].Vtx[m++] = new Vertex(new Vector3(-i.X2, i.Y2, i.Z1), cur_color);
                vtx[2].Vtx[m++] = new Vertex(new Vector3(-i.X1, i.Y2, i.Z1), cur_color);
                vtx[2].Vtx[m++] = new Vertex(new Vector3(-i.X1, i.Y1, i.Z1), cur_color);
                vtx[2].Vtx[m++] = new Vertex(new Vector3(-i.X1, i.Y1, i.Z2), cur_color);
                vtx[2].Vtx[m++] = new Vertex(new Vector3(-i.X2, i.Y1, i.Z2), cur_color);
                vtx[2].Vtx[m++] = new Vertex(new Vector3(-i.X2, i.Y1, i.Z1), cur_color);
                vtx[2].VtxOffs[l++] = m;
                vtx[2].Vtx[m++] = new Vertex(new Vector3(-i.X1, i.Y1, i.Z2), cur_color);
                vtx[2].Vtx[m++] = new Vertex(new Vector3(-i.X1, i.Y2, i.Z2), cur_color);
                vtx[2].Vtx[m++] = new Vertex(new Vector3(-i.X2, i.Y2, i.Z2), cur_color);
                vtx[2].Vtx[m++] = new Vertex(new Vector3(-i.X2, i.Y1, i.Z2), cur_color);
                vtx[2].VtxOffs[l++] = m;
                vtx[2].Vtx[m++] = new Vertex(new Vector3(-i.X1, i.Y2, i.Z2), cur_color);
                vtx[2].Vtx[m++] = new Vertex(new Vector3(-i.X1, i.Y2, i.Z1), cur_color);
                vtx[2].VtxOffs[l++] = m;
                vtx[2].Vtx[m++] = new Vertex(new Vector3(-i.X2, i.Y2, i.Z2), cur_color);
                vtx[2].Vtx[m++] = new Vertex(new Vector3(-i.X2, i.Y2, i.Z1), cur_color);
                min_x = Math.Min(min_x, i.X1);
                min_y = Math.Min(min_y, i.Y1);
                min_z = Math.Min(min_z, i.Z1);
                max_x = Math.Max(max_x, i.X2);
                max_y = Math.Max(max_y, i.Y2);
                max_z = Math.Max(max_z, i.Z2);
            }
            zFar = Math.Max(zFar, Math.Max(max_x - min_x, Math.Max(max_y - min_y, max_z - min_z)));
            UpdateVBO(2);
        }

        public void LoadSkydome()
        {
            float min_x = float.MaxValue, min_y = float.MaxValue, min_z = float.MaxValue, max_x = float.MinValue, max_y = float.MinValue, max_z = float.MinValue;
            SceneryDataController scenery_sec = file.AuxCont.GetItem<SceneryDataController>(0);
            SectionController skydome_sec = file.AuxCont.GetItem<SectionController>(6).GetItem<SectionController>(8);
            SectionController mesh_sec = file.AuxCont.GetItem<SectionController>(6).GetItem<SectionController>(2);
            SectionController model_sec = file.AuxCont.GetItem<SectionController>(6).GetItem<SectionController>(6);
            SectionController special_sec = file.AuxCont.GetItem<SectionController>(6).GetItem<SectionController>(7);
            if (skydome_sec.Data.ContainsItem(scenery_sec.Data.SkydomeID))
            {
                SkydomeController sky = skydome_sec.GetItem<SkydomeController>(scenery_sec.Data.SkydomeID);
                for (int i = 0; i < sky.Data.ModelIDs.Length; ++i)
                {
                    if (special_sec.Data.ContainsItem(sky.Data.ModelIDs[i]))
                        continue;
                    MeshController mesh = mesh_sec.GetItem<MeshController>(model_sec.GetItem<ModelController>(sky.Data.ModelIDs[i]).Data.MeshID);
                    mesh.LoadMeshData();

                    Vertex[] vbuffer = new Vertex[mesh.Vertices.Length];

                    for (int v = 0; v < mesh.Vertices.Length; v++)
                    {
                        vbuffer[v] = mesh.Vertices[v];
                        mesh.Vertices[v].Pos = new Vector3(mesh.Vertices[v].Pos.X, mesh.Vertices[v].Pos.Y, mesh.Vertices[v].Pos.Z);
                        mesh.Vertices[v].Pos *= 100f; // skydome scale, the value is just a guess
                    }

                    foreach (var v in mesh.Vertices)
                    {
                        min_x = Math.Min(min_x, v.Pos.X);
                        min_y = Math.Min(min_y, v.Pos.Y);
                        min_z = Math.Min(min_z, v.Pos.Z);
                        max_x = Math.Max(max_x, v.Pos.X);
                        max_y = Math.Max(max_y, v.Pos.Y);
                        max_z = Math.Max(max_z, v.Pos.Z);
                    }
                    vtx[reserved_layers + static_layers] = new VertexBufferData();
                    vtx[reserved_layers + static_layers].Vtx = mesh.Vertices;
                    vtx[reserved_layers + static_layers].VtxInd = mesh.Indices;
                    vtx[reserved_layers + static_layers].Type = VertexBufferData.BufferType.Skydome;
                    mesh.Vertices = vbuffer;
                    UpdateVBO(reserved_layers + static_layers);
                    static_layers++;
                }
                zFar = Math.Max(zFar, Math.Max(max_x - min_x, Math.Max(max_y - min_y, max_z - min_z)));
            }
        }

        public void LoadScenery(FileController file, bool isLinkedScenery, Matrix4 ChunkMatrix, int LinkID, bool isUpdate)
        {
            SceneryDataController scenery_sec = file.GetItem<SceneryDataController>(0);

            if (scenery_sec.Data.SceneryRoot == null)
            {
                return;
            }

            LoadSceneryStruct(scenery_sec.Data.SceneryRoot, file, isLinkedScenery, ChunkMatrix, LinkID, isUpdate);
        }

        public void LoadSceneryStruct(SceneryData.SceneryStruct ptr, FileController file, bool isLinkedScenery, Matrix4 ChunkMatrix, int LinkID, bool isUpdate)
        {
            LoadSceneryModel(ptr.Model, file, isLinkedScenery, ChunkMatrix, LinkID, isUpdate);
            for (int i = 0; i < ptr.Links.Length; i++)
            {
                if (ptr.Links[i] is SceneryData.SceneryModelStruct)
                {
                    LoadSceneryModel((SceneryData.SceneryModelStruct)ptr.Links[i], file, isLinkedScenery, ChunkMatrix, LinkID, isUpdate);
                }
                else if (ptr.Links[i] is SceneryData.SceneryStruct)
                {
                    LoadSceneryStruct((SceneryData.SceneryStruct)ptr.Links[i], file, isLinkedScenery, ChunkMatrix, LinkID, isUpdate);
                }
            }
        }

        public void LoadSceneryModel(SceneryData.SceneryModelStruct ptr, FileController file, bool isLinkedScenery, Matrix4 ChunkMatrix, int LinkID, bool isUpdate)
        {
            float min_x = float.MaxValue, min_y = float.MaxValue, min_z = float.MaxValue, max_x = float.MinValue, max_y = float.MinValue, max_z = float.MinValue;
            SceneryDataController scenery_sec = file.GetItem<SceneryDataController>(0);
            SectionController graphics_sec = file.GetItem<SectionController>(6);
            SectionController tex_sec = graphics_sec.GetItem<SectionController>(0);
            SectionController mat_sec = graphics_sec.GetItem<SectionController>(1);
            SectionController mesh_sec = graphics_sec.GetItem<SectionController>(2);
            SectionController model_sec = graphics_sec.GetItem<SectionController>(6);
            SectionController special_sec = graphics_sec.GetItem<SectionController>(7);

            for (int m = 0; m < ptr.Models.Count; m++)
            {
                MeshController mesh;
                MaterialController[] mat;
                TextureController[] tex;
                uint modelID = 0;
                if (!ptr.Models[m].isSpecial)
                {
                    modelID = ptr.Models[m].ModelID;
                }
                else
                {
                    uint LODcount = special_sec.Data.GetItem<SpecialModel>(ptr.Models[m].ModelID).K_Count;
                    int targetLOD = 3;
                    if (LODcount > 3)
                    {
                        targetLOD = 0;
                    }
                    else if (LODcount > 2)
                    {
                        targetLOD = 1;
                    }
                    else if (LODcount > 1)
                    {
                        targetLOD = 2;
                    }
                    modelID = special_sec.Data.GetItem<SpecialModel>(ptr.Models[m].ModelID).LODModelIDs[targetLOD];
                }
                mesh = mesh_sec.GetItem<MeshController>(model_sec.GetItem<ModelController>(modelID).Data.MeshID);

                int matCount = model_sec.GetItem<ModelController>(modelID).Data.MaterialIDs.Length;
                mat = new MaterialController[matCount];
                tex = new TextureController[matCount];

                if (file.Data.Type == TwinsFile.FileType.SM2)
                {
                    for (int t = 0; t < matCount; t++)
                    {
                        mat[t] = mat_sec.GetItem<MaterialController>(model_sec.GetItem<ModelController>(modelID).Data.MaterialIDs[t]);
                        if (mat_sec.GetItem<MaterialController>(mat[t].Data.ID).Data.Tex != 0)
                        {
                            tex[t] = tex_sec.GetItem<TextureController>(mat_sec.GetItem<MaterialController>(mat[t].Data.ID).Data.Tex);
                        }
                        else
                        {
                            tex[t] = null;
                        }
                    }
                }
                //textures loaded into tex[]... but what next?

                mesh.LoadMeshData();

                Matrix4 modelMatrix = Matrix4.Identity;

                // closest: -M11, -M21, -M31, -X

                // Rotation
                modelMatrix.M11 = -ptr.Models[m].ModelMatrix[0].X;
                modelMatrix.M12 = ptr.Models[m].ModelMatrix[1].X;
                modelMatrix.M13 = ptr.Models[m].ModelMatrix[2].X;

                modelMatrix.M21 = -ptr.Models[m].ModelMatrix[0].Y;
                modelMatrix.M22 = ptr.Models[m].ModelMatrix[1].Y;
                modelMatrix.M23 = ptr.Models[m].ModelMatrix[2].Y;

                modelMatrix.M31 = -ptr.Models[m].ModelMatrix[0].Z;
                modelMatrix.M32 = ptr.Models[m].ModelMatrix[1].Z;
                modelMatrix.M33 = ptr.Models[m].ModelMatrix[2].Z;

                modelMatrix.M14 = ptr.Models[m].ModelMatrix[0].W;
                modelMatrix.M24 = ptr.Models[m].ModelMatrix[1].W;
                modelMatrix.M34 = ptr.Models[m].ModelMatrix[2].W;

                // Position
                modelMatrix.M41 = ptr.Models[m].ModelMatrix[3].X;
                modelMatrix.M42 = ptr.Models[m].ModelMatrix[3].Y;
                modelMatrix.M43 = ptr.Models[m].ModelMatrix[3].Z;
                modelMatrix.M44 = ptr.Models[m].ModelMatrix[3].W;

                modelMatrix *= Matrix4.CreateScale(-1, 1, 1);

                Vertex[] vbuffer = new Vertex[mesh.Vertices.Length];

                for (int v = 0; v < mesh.Vertices.Length; v++)
                {
                    vbuffer[v] = mesh.Vertices[v];
                    Vector4 vertexPos = new Vector4(mesh.Vertices[v].Pos.X, mesh.Vertices[v].Pos.Y, mesh.Vertices[v].Pos.Z, 1);
                    vertexPos *= modelMatrix;
                    if (isLinkedScenery)
                    {
                        vertexPos *= ChunkMatrix;
                    }
                    mesh.Vertices[v].Pos = new Vector3(vertexPos.X, vertexPos.Y, vertexPos.Z);
                }

                foreach (var v in mesh.Vertices)
                {
                    min_x = Math.Min(min_x, v.Pos.X);
                    min_y = Math.Min(min_y, v.Pos.Y);
                    min_z = Math.Min(min_z, v.Pos.Z);
                    max_x = Math.Max(max_x, v.Pos.X);
                    max_y = Math.Max(max_y, v.Pos.Y);
                    max_z = Math.Max(max_z, v.Pos.Z);
                }

                int vtx_id = reserved_layers + static_layers;
                if (isUpdate)
                {
                    vtx_id = scenery_layer + scenery_starting_layer;
                }

                vtx[vtx_id] = new VertexBufferData();
                vtx[vtx_id].Vtx = mesh.Vertices;
                vtx[vtx_id].VtxInd = mesh.Indices;
                if (isLinkedScenery)
                {
                    vtx[vtx_id].Type = VertexBufferData.BufferType.ExtraScenery;
                    vtx[vtx_id].LinkID = LinkID;
                }
                else
                {
                    vtx[vtx_id].Type = VertexBufferData.BufferType.Scenery;
                }
                mesh.Vertices = vbuffer;
                UpdateVBO(vtx_id);
                if (!isUpdate)
                {
                    static_layers++;
                }
                else
                {
                    scenery_layer++;
                }
            }

            zFar = Math.Max(zFar, Math.Max(max_x - min_x, Math.Max(max_y - min_y, max_z - min_z)));
        }

        public void LoadDynamicScenery(FileController file, bool isLinkedScenery, Matrix4 ChunkMatrix, int LinkID)
        {
            float min_x = float.MaxValue, min_y = float.MaxValue, min_z = float.MaxValue, max_x = float.MinValue, max_y = float.MinValue, max_z = float.MinValue;
            DynamicSceneryDataController scenery_sec = file.GetItem<DynamicSceneryDataController>(4);
            SectionController mesh_sec = file.GetItem<SectionController>(6).GetItem<SectionController>(2);
            SectionController model_sec = file.GetItem<SectionController>(6).GetItem<SectionController>(6);
            SectionController special_sec = file.GetItem<SectionController>(6).GetItem<SectionController>(7);

            if (scenery_sec.Data.Size < 12)
            {
                return;
            }
            
            if (scenery_sec.Data.Models.Count <= 0)
            {
                return;
            }
            for (int s = 0; s < scenery_sec.Data.Models.Count; s++)
            {
                MeshController mesh = mesh_sec.GetItem<MeshController>(model_sec.GetItem<ModelController>(scenery_sec.Data.Models[s].ModelID).Data.MeshID);
                mesh.LoadMeshData();

                //Matrix3 rot_ins = Matrix3.Identity;
                //rot_ins *= Matrix3.CreateRotationX(scenery_sec.Data.Models[s].LocalRotation[0] * MathHelper.TwoPi);
                //rot_ins *= Matrix3.CreateRotationY(scenery_sec.Data.Models[s].LocalRotation[1] * MathHelper.TwoPi);
                //rot_ins *= Matrix3.CreateRotationZ(scenery_sec.Data.Models[s].LocalRotation[2] * MathHelper.TwoPi);
                // 404: rotation NOT FOUND!!

                Vector4 pos_ins = scenery_sec.Data.Models[s].WorldPosition.ToVec4();
                pos_ins.X = -pos_ins.X;

                Vertex[] vbuffer = new Vertex[mesh.Vertices.Length];

                for (int v = 0; v < mesh.Vertices.Length; v++)
                {
                    vbuffer[v] = mesh.Vertices[v];
                    Vector4 vertexPos = new Vector4(mesh.Vertices[v].Pos.X, mesh.Vertices[v].Pos.Y, mesh.Vertices[v].Pos.Z, 1);
                    //vertexPos *= rot_ins;
                    if (isLinkedScenery)
                    {
                        vertexPos *= ChunkMatrix;
                    }
                    vertexPos += pos_ins;
                    
                    mesh.Vertices[v].Pos = new Vector3(vertexPos.X, vertexPos.Y, vertexPos.Z);
                }

                foreach (var v in mesh.Vertices)
                {
                    min_x = Math.Min(min_x, v.Pos.X);
                    min_y = Math.Min(min_y, v.Pos.Y);
                    min_z = Math.Min(min_z, v.Pos.Z);
                    max_x = Math.Max(max_x, v.Pos.X);
                    max_y = Math.Max(max_y, v.Pos.Y);
                    max_z = Math.Max(max_z, v.Pos.Z);
                }
                vtx[reserved_layers + static_layers] = new VertexBufferData();
                vtx[reserved_layers + static_layers].Vtx = mesh.Vertices;
                vtx[reserved_layers + static_layers].VtxInd = mesh.Indices;
                if (isLinkedScenery)
                {
                    vtx[reserved_layers + static_layers].Type = VertexBufferData.BufferType.ExtraScenery;
                    vtx[reserved_layers + static_layers].LinkID = LinkID;
                }
                else
                {
                    vtx[reserved_layers + static_layers].Type = VertexBufferData.BufferType.Scenery;
                }
                mesh.Vertices = vbuffer;
                UpdateVBO(reserved_layers + static_layers);
                static_layers++;
            }
            

            zFar = Math.Max(zFar, Math.Max(max_x - min_x, Math.Max(max_y - min_y, max_z - min_z)));
        }

        public void LoadAllLinkedChunks()
        {
            string pathMod = "2";
            if (file.Data.Type == TwinsFile.FileType.RMX)
            {
                pathMod = "x";
            }
            string origPath = file.DataAux.FileName;
            SceneryDataController scenery_sec = file.AuxCont.GetItem<SceneryDataController>(0);
            string curChunkPath = scenery_sec.Data.ChunkName;
            string adjustedPath = origPath.Substring(0, origPath.Length - curChunkPath.Length - 4);
            for (int i = 0; i < links.Links.Count; i++)
            {
                string ChunkName = new string(links.Links[i].Path);
                string Path = adjustedPath + ChunkName + ".sm" + pathMod;
                if (System.IO.File.Exists(Path))
                {
                    TwinsFile linkfile = new TwinsFile();
                    linkfile.LoadFile(Path, file.DataAux.Type);
                    FileController linkCont = new FileController(null, linkfile, null);
                    foreach (var a in linkfile.Records)
                    {
                        GenTreeNode(a, linkCont, linkCont);
                    }

                    Pos[] ChunkOffset = links.Links[i].ChunkMatrix;
                    Matrix4 ChunkMatrix = Matrix4.Identity;
                    ChunkMatrix.M11 = ChunkOffset[0].X;
                    ChunkMatrix.M12 = ChunkOffset[1].X;
                    ChunkMatrix.M13 = ChunkOffset[2].X;

                    ChunkMatrix.M21 = ChunkOffset[0].Y;
                    ChunkMatrix.M22 = ChunkOffset[1].Y;
                    ChunkMatrix.M23 = ChunkOffset[2].Y;

                    ChunkMatrix.M31 = ChunkOffset[0].Z;
                    ChunkMatrix.M32 = ChunkOffset[1].Z;
                    ChunkMatrix.M33 = ChunkOffset[2].Z;

                    ChunkMatrix.M14 = ChunkOffset[0].W;
                    ChunkMatrix.M24 = ChunkOffset[1].W;
                    ChunkMatrix.M34 = ChunkOffset[2].W;

                    ChunkMatrix.M41 = -ChunkOffset[3].X;
                    ChunkMatrix.M42 = ChunkOffset[3].Y;
                    ChunkMatrix.M43 = ChunkOffset[3].Z;
                    ChunkMatrix.M44 = ChunkOffset[3].W;

                    if (file.Data.Type != TwinsFile.FileType.RMX)
                    {
                        LoadScenery(linkCont, true, ChunkMatrix, i, false);
                        LoadDynamicScenery(linkCont, true, ChunkMatrix, i);
                    }

                    string Path2 = adjustedPath + ChunkName + ".rm" + pathMod;
                    if (System.IO.File.Exists(Path))
                    {
                        TwinsFile linkfile2 = new TwinsFile();
                        linkfile2.LoadFile(Path2, file.Data.Type);
                        FileController linkCont2 = new FileController(null, linkfile2, null);
                        foreach (var a in linkfile2.Records)
                        {
                            GenTreeNode(a, linkCont2, linkCont2);
                        }

                        for (int c = 0; c < 28; c++)
                        {
                            LoadColTree(linkCont2, c, true, ChunkMatrix, i);
                        }
                    }
                }
            }
        }

        public void UpdateScenery()
        {
            scenery_layer = 0;
            LoadScenery(file.AuxCont, false, new Matrix4(), 0, true);
        }

        public void GenTreeNode(TwinsItem a, Controller controller, FileController targetFile)
        {
            Controller c;
            if (a is TwinsSection)
            {
                c = new SectionController(null, (TwinsSection)a, targetFile);
                foreach (var i in ((TwinsSection)a).Records)
                {
                    GenTreeNode(i, c, targetFile);
                }
            }
            else if (a is Texture)
                c = new TextureController(null, (Texture)a, targetFile);
            else if (a is Material)
                c = new MaterialController(null, (Material)a, targetFile);
            else if (a is Mesh)
                c = new MeshController(null, (Mesh)a, targetFile);
            else if (a is Model)
                c = new ModelController(null, (Model)a, targetFile);
            else if (a is Skydome)
                c = new SkydomeController(null, (Skydome)a, targetFile);
            else if (a is GameObject)
                c = new ObjectController(null, (GameObject)a, targetFile);
            else if (a is Script)
                c = new ScriptController(null, (Script)a, targetFile);
            else if (a is SoundEffect)
                c = new SEController(null, (SoundEffect)a, targetFile);
            else if (a is AIPosition)
                c = new AIPositionController(null, (AIPosition)a, targetFile);
            else if (a is AIPath)
                c = new AIPathController(null, (AIPath)a, targetFile);
            else if (a is Position)
                c = new PositionController(null, (Position)a, targetFile);
            else if (a is Twinsanity.Path)
                c = new PathController(null, (Twinsanity.Path)a, targetFile);
            else if (a is Instance)
                c = new InstanceController(null, (Instance)a, targetFile);
            else if (a is Trigger)
                c = new TriggerController(null, (Trigger)a, targetFile);
            else if (a is ColData)
                c = new ColDataController(null, (ColData)a, targetFile);
            else if (a is ChunkLinks)
                c = new ChunkLinksController(null, (ChunkLinks)a, targetFile);
            else if (a is GraphicsInfo)
                c = new GraphicsInfoController(null, (GraphicsInfo)a, targetFile);
            else if (a is ArmatureModel)
                c = new ArmatureModelController(null, (ArmatureModel)a, targetFile);
            else if (a is ArmatureModelX)
                c = new ArmatureModelXController(null, (ArmatureModelX)a, targetFile);
            else if (a is MaterialDemo)
                c = new MaterialDController(null, (MaterialDemo)a, targetFile);
            else if (a is SceneryData)
                c = new SceneryDataController(null, (SceneryData)a, targetFile);
            else if (a is SpecialModel)
                c = new SpecialModelController(null, (SpecialModel)a, targetFile);
            else if (a is ParticleData)
                c = new ParticleDataController(null, (ParticleData)a, targetFile);
            else if (a is DynamicSceneryData)
                c = new DynamicSceneryDataController(null, (DynamicSceneryData)a, targetFile);
            else if (a is MeshX)
                c = new MeshXController(null, (MeshX)a, targetFile);
            else if (a is CollisionSurface)
                c = new CollisionSurfaceController(null, (CollisionSurface)a, targetFile);
            else if (a is Camera)
                c = new CameraController(null, (Camera)a, targetFile);
            else if (a is InstanceTemplate)
                c = new InstaceTemplateController(null, (InstanceTemplate)a, targetFile);
            else if (a is InstanceTemplateDemo)
                c = new InstaceTemplateDemoController(null, (InstanceTemplateDemo)a, targetFile);
            else if (a is InstanceDemo)
                c = new InstanceDemoController(null, (InstanceDemo)a, targetFile);
            else if (a is GameObjectDemo)
                c = new ObjectDemoController(null, (GameObjectDemo)a, targetFile);
            else if (a is Animation)
                c = new AnimationController(null, (Animation)a, targetFile);
            else if (a is CodeModel)
                c = new CodeModelController(null, (CodeModel)a, targetFile);
            else
                c = new ItemController(null, a, targetFile);

            controller.AddNode(c);
        }

        public void LoadPositions()
        {
            float min_x = float.MaxValue, min_y = float.MaxValue, min_z = float.MaxValue, max_x = float.MinValue, max_y = float.MinValue, max_z = float.MinValue;
            bool[] record_exists = new bool[8];
            int posi_count = 0;
            for (uint i = 0; i <= 7; ++i)
            {
                record_exists[i] = file.Data.ContainsItem(i);
                if (record_exists[i])
                {
                    if (file.Data.GetItem<TwinsSection>(i).ContainsItem(3))
                    {
                        posi_count += file.Data.GetItem<TwinsSection>(i).GetItem<TwinsSection>(3).Records.Count;
                        record_exists[i] = true;
                    }
                    else
                        record_exists[i] = false;
                }
            }
            if (vtx[3] == null || vtx.Length != (circle_res * 3 + 6) * posi_count)
            {
                vtx[3].VtxCounts = new int[6 * posi_count];
                vtx[3].VtxOffs = new int[6 * posi_count];
                vtx[3].Vtx = new Vertex[(circle_res * 3 + 6) * posi_count];
                for (int i = 0; i < posi_count; ++i)
                {
                    vtx[3].VtxCounts[i * 6 + 0] = 2;
                    vtx[3].VtxCounts[i * 6 + 1] = 2;
                    vtx[3].VtxCounts[i * 6 + 2] = 2;
                    vtx[3].VtxCounts[i * 6 + 3] = circle_res;
                    vtx[3].VtxCounts[i * 6 + 4] = circle_res;
                    vtx[3].VtxCounts[i * 6 + 5] = circle_res;
                }
            }
            int l = 0, m = 0;
            for (uint i = 0; i <= 7; ++i)
            {
                if (!record_exists[i]) continue;
                if (file.Data.GetItem<TwinsSection>(i).ContainsItem(3))
                {
                    foreach (Position pos in file.Data.GetItem<TwinsSection>(i).GetItem<TwinsSection>(3).Records)
                    {
                        Vector3 pos_pos = pos.Pos.ToVec3();
                        pos_pos.X = -pos_pos.X;
                        vtx[3].VtxOffs[l++] = m;
                        vtx[3].Vtx[m++] = new Vertex(new Vector3(-indicator_size * 0.75f * 0.5f, 0, 0) + pos_pos, Color.Red);
                        vtx[3].Vtx[m++] = new Vertex(new Vector3(+indicator_size * 0.375f * 0.5f, 0, 0) + pos_pos, Color.Red);
                        vtx[3].VtxOffs[l++] = m;
                        vtx[3].Vtx[m++] = new Vertex(new Vector3(0, indicator_size * 0.75f * 0.5f, 0) + pos_pos, Color.Green);
                        vtx[3].Vtx[m++] = new Vertex(new Vector3(0, -indicator_size * 0.375f * 0.5f, 0) + pos_pos, Color.Green);
                        vtx[3].VtxOffs[l++] = m;
                        vtx[3].Vtx[m++] = new Vertex(new Vector3(0, 0, indicator_size * 0.75f * 0.5f) + pos_pos, Color.Blue);
                        vtx[3].Vtx[m++] = new Vertex(new Vector3(0, 0, -indicator_size * 0.375f * 0.5f) + pos_pos, Color.Blue);
                        Color cur_color = (file.SelectedItem == pos) ? Color.White : colors[colors.Length - i * 2 - 1];
                        vtx[3].VtxOffs[l++] = m;
                        for (int j = 0; j < circle_res; ++j)
                        {
                            Vector3 vec = new Vector3(0, 0, indicator_size);
                            vec *= Matrix3.Identity * Matrix3.CreateRotationX(MathHelper.TwoPi / circle_res * j);
                            vtx[3].Vtx[m++] = new Vertex(pos_pos + vec, cur_color);
                        }
                        vtx[3].VtxOffs[l++] = m;
                        for (int j = 0; j < circle_res; ++j)
                        {
                            Vector3 vec = new Vector3(0, 0, indicator_size);
                            vec *= Matrix3.Identity * Matrix3.CreateRotationY(MathHelper.TwoPi / circle_res * j);
                            vtx[3].Vtx[m++] = new Vertex(pos_pos + vec, cur_color);
                        }
                        vtx[3].VtxOffs[l++] = m;
                        for (int j = 0; j < circle_res; ++j)
                        {
                            Vector3 vec = new Vector3(0, indicator_size, 0);
                            vec *= Matrix3.Identity * Matrix3.CreateRotationZ(MathHelper.TwoPi / circle_res * j);
                            vtx[3].Vtx[m++] = new Vertex(pos_pos + vec, cur_color);
                        }
                        min_x = Math.Min(min_x, pos_pos.X);
                        min_y = Math.Min(min_y, pos_pos.Y);
                        min_z = Math.Min(min_z, pos_pos.Z);
                        max_x = Math.Max(max_x, pos_pos.X);
                        max_y = Math.Max(max_y, pos_pos.Y);
                        max_z = Math.Max(max_z, pos_pos.Z);
                    }
                }
            }
            zFar = Math.Max(zFar, Math.Max(max_x - min_x, Math.Max(max_y - min_y, max_z - min_z)));
            UpdateVBO(3);
        }

        public void LoadParticles()
        {
            float min_x = float.MaxValue, min_y = float.MaxValue, min_z = float.MaxValue, max_x = float.MinValue, max_y = float.MinValue, max_z = float.MinValue;
            bool record_exists = false;
            uint posi_count = 0;
            record_exists = file.Data.ContainsItem(8);
            if (!record_exists)
            {
                return;
            }
            ParticleData partData = file.Data.GetItem<ParticleData>(8);
            posi_count = partData.ParticleInstanceCount;
            if (posi_count <= 0)
            {
                return;
            }
            if (vtx[5] == null || vtx.Length != (circle_res * 3 + 6) * posi_count)
            {
                vtx[5].VtxCounts = new int[6 * posi_count];
                vtx[5].VtxOffs = new int[6 * posi_count];
                vtx[5].Vtx = new Vertex[(circle_res * 3 + 6) * posi_count];
                for (int i = 0; i < posi_count; ++i)
                {
                    vtx[5].VtxCounts[i * 6 + 0] = 2;
                    vtx[5].VtxCounts[i * 6 + 1] = 2;
                    vtx[5].VtxCounts[i * 6 + 2] = 2;
                    vtx[5].VtxCounts[i * 6 + 3] = circle_res;
                    vtx[5].VtxCounts[i * 6 + 4] = circle_res;
                    vtx[5].VtxCounts[i * 6 + 5] = circle_res;
                }
            }
            int l = 0, m = 0;
            for (int i = 0; i < partData.ParticleInstances.Count; i++)
            {
                Matrix3 rot_ins = Matrix3.Identity;
                rot_ins *= Matrix3.CreateRotationX(partData.ParticleInstances[i].Rot_X / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                rot_ins *= Matrix3.CreateRotationY(-partData.ParticleInstances[i].Rot_Y / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                rot_ins *= Matrix3.CreateRotationZ(-partData.ParticleInstances[i].Rot_Z / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi);
                Vector3 pos_pos = partData.ParticleInstances[i].Position.ToVec3();
                pos_pos.X = -pos_pos.X;
                vtx[5].VtxOffs[l++] = m;
                vtx[5].Vtx[m++] = new Vertex(new Vector3(-indicator_size * 0.75f * 0.5f, 0, 0) * rot_ins + pos_pos, Color.Red);
                vtx[5].Vtx[m++] = new Vertex(new Vector3(+indicator_size * 0.375f * 0.5f, 0, 0) * rot_ins + pos_pos, Color.Red);
                vtx[5].VtxOffs[l++] = m;
                vtx[5].Vtx[m++] = new Vertex(new Vector3(0, indicator_size * 0.75f * 0.5f, 0) * rot_ins + pos_pos, Color.Green);
                vtx[5].Vtx[m++] = new Vertex(new Vector3(0, -indicator_size * 0.375f * 0.5f, 0) * rot_ins + pos_pos, Color.Green);
                vtx[5].VtxOffs[l++] = m;
                vtx[5].Vtx[m++] = new Vertex(new Vector3(0, 0, indicator_size * 0.75f * 0.5f) * rot_ins + pos_pos, Color.Blue);
                vtx[5].Vtx[m++] = new Vertex(new Vector3(0, 0, -indicator_size * 0.375f * 0.5f) * rot_ins + pos_pos, Color.Blue);
                Color cur_color = Color.Pink;
                vtx[5].VtxOffs[l++] = m;
                for (int j = 0; j < circle_res; ++j)
                {
                    Vector3 vec = new Vector3(0, 0, indicator_size);
                    vec *= Matrix3.Identity * Matrix3.CreateRotationX(MathHelper.TwoPi / circle_res * j);
                    vtx[5].Vtx[m++] = new Vertex(pos_pos + vec, cur_color);
                }
                vtx[5].VtxOffs[l++] = m;
                for (int j = 0; j < circle_res; ++j)
                {
                    Vector3 vec = new Vector3(0, 0, indicator_size);
                    vec *= Matrix3.Identity * Matrix3.CreateRotationY(MathHelper.TwoPi / circle_res * j);
                    vtx[5].Vtx[m++] = new Vertex(pos_pos + vec, cur_color);
                }
                vtx[5].VtxOffs[l++] = m;
                for (int j = 0; j < circle_res; ++j)
                {
                    Vector3 vec = new Vector3(0, indicator_size, 0);
                    vec *= Matrix3.Identity * Matrix3.CreateRotationZ(MathHelper.TwoPi / circle_res * j);
                    vtx[5].Vtx[m++] = new Vertex(pos_pos + vec, cur_color);
                }
                min_x = Math.Min(min_x, pos_pos.X);
                min_y = Math.Min(min_y, pos_pos.Y);
                min_z = Math.Min(min_z, pos_pos.Z);
                max_x = Math.Max(max_x, pos_pos.X);
                max_y = Math.Max(max_y, pos_pos.Y);
                max_z = Math.Max(max_z, pos_pos.Z);
            }
            zFar = Math.Max(zFar, Math.Max(max_x - min_x, Math.Max(max_y - min_y, max_z - min_z)));
            UpdateVBO(5);
        }

        public void LoadLights()
        {
            float min_x = float.MaxValue, min_y = float.MaxValue, min_z = float.MaxValue, max_x = float.MinValue, max_y = float.MinValue, max_z = float.MinValue;
            uint posi_count = 0;
            if (file.DataAux == null)
            {
                return;
            }
            SceneryData partData = file.DataAux.GetItem<SceneryData>(0);
            posi_count = (uint)partData.LightsPoint.Count + (uint)partData.LightsAmbient.Count + (uint)partData.LightsDirectional.Count + (uint)partData.LightsNegative.Count;
            if (posi_count <= 0)
            {
                return;
            }
            if (vtx[6] == null)
            {
                vtx[6] = new VertexBufferData();
                vtx[6].VtxCounts = new int[6 * posi_count];
                vtx[6].VtxOffs = new int[6 * posi_count];
                vtx[6].Vtx = new Vertex[(circle_res * 3 + 6) * posi_count];
                for (int i = 0; i < posi_count; ++i)
                {
                    vtx[6].VtxCounts[i * 6 + 0] = 2;
                    vtx[6].VtxCounts[i * 6 + 1] = 2;
                    vtx[6].VtxCounts[i * 6 + 2] = 2;
                    vtx[6].VtxCounts[i * 6 + 3] = circle_res;
                    vtx[6].VtxCounts[i * 6 + 4] = circle_res;
                    vtx[6].VtxCounts[i * 6 + 5] = circle_res;
                }
            }
            int l = 0, m = 0;
            for (int i = 0; i < partData.LightsAmbient.Count; i++)
            {
                Matrix3 rot_ins = Matrix3.Identity;
                Vector3 pos_pos = partData.LightsAmbient[i].Position.ToVec3();
                pos_pos.X = -pos_pos.X;
                float LightSize = partData.LightsAmbient[i].Radius / 10f;
                vtx[6].VtxOffs[l++] = m;
                vtx[6].Vtx[m++] = new Vertex(new Vector3(-indicator_size * 0.75f * 0.5f, 0, 0) * rot_ins + pos_pos, Color.Red);
                vtx[6].Vtx[m++] = new Vertex(new Vector3(+indicator_size * 0.375f * 0.5f, 0, 0) * rot_ins + pos_pos, Color.Red);
                vtx[6].VtxOffs[l++] = m;
                vtx[6].Vtx[m++] = new Vertex(new Vector3(0, indicator_size * 0.75f * 0.5f, 0) * rot_ins + pos_pos, Color.Green);
                vtx[6].Vtx[m++] = new Vertex(new Vector3(0, -indicator_size * 0.375f * 0.5f, 0) * rot_ins + pos_pos, Color.Green);
                vtx[6].VtxOffs[l++] = m;
                vtx[6].Vtx[m++] = new Vertex(new Vector3(0, 0, indicator_size * 0.75f * 0.5f) * rot_ins + pos_pos, Color.Blue);
                vtx[6].Vtx[m++] = new Vertex(new Vector3(0, 0, -indicator_size * 0.375f * 0.5f) * rot_ins + pos_pos, Color.Blue);
                Color cur_color = Color.FromArgb(255, (int)(partData.LightsAmbient[i].Color_R * 255), (int)(partData.LightsAmbient[i].Color_G * 255), (int)(partData.LightsAmbient[i].Color_B * 255));  //Color.White;
                vtx[6].VtxOffs[l++] = m;
                for (int j = 0; j < circle_res; ++j)
                {
                    Vector3 vec = new Vector3(0, 0, LightSize);
                    vec *= Matrix3.Identity * Matrix3.CreateRotationX(MathHelper.TwoPi / circle_res * j);
                    vtx[6].Vtx[m++] = new Vertex(pos_pos + vec, cur_color);
                }
                vtx[6].VtxOffs[l++] = m;
                for (int j = 0; j < circle_res; ++j)
                {
                    Vector3 vec = new Vector3(0, 0, LightSize);
                    vec *= Matrix3.Identity * Matrix3.CreateRotationY(MathHelper.TwoPi / circle_res * j);
                    vtx[6].Vtx[m++] = new Vertex(pos_pos + vec, cur_color);
                }
                vtx[6].VtxOffs[l++] = m;
                for (int j = 0; j < circle_res; ++j)
                {
                    Vector3 vec = new Vector3(0, LightSize, 0);
                    vec *= Matrix3.Identity * Matrix3.CreateRotationZ(MathHelper.TwoPi / circle_res * j);
                    vtx[6].Vtx[m++] = new Vertex(pos_pos + vec, cur_color);
                }
                min_x = Math.Min(min_x, pos_pos.X);
                min_y = Math.Min(min_y, pos_pos.Y);
                min_z = Math.Min(min_z, pos_pos.Z);
                max_x = Math.Max(max_x, pos_pos.X);
                max_y = Math.Max(max_y, pos_pos.Y);
                max_z = Math.Max(max_z, pos_pos.Z);
            }
            for (int i = 0; i < partData.LightsDirectional.Count; i++)
            {
                Matrix3 rot_ins = Matrix3.Identity;
                Vector3 pos_pos = partData.LightsDirectional[i].Position.ToVec3();
                pos_pos.X = -pos_pos.X;
                float LightSize = partData.LightsDirectional[i].Radius / 10f;
                vtx[6].VtxOffs[l++] = m;
                vtx[6].Vtx[m++] = new Vertex(new Vector3(-indicator_size * 0.75f * 0.5f, 0, 0) * rot_ins + pos_pos, Color.Red);
                vtx[6].Vtx[m++] = new Vertex(new Vector3(+indicator_size * 0.375f * 0.5f, 0, 0) * rot_ins + pos_pos, Color.Red);
                vtx[6].VtxOffs[l++] = m;
                vtx[6].Vtx[m++] = new Vertex(new Vector3(0, indicator_size * 0.75f * 0.5f, 0) * rot_ins + pos_pos, Color.Green);
                vtx[6].Vtx[m++] = new Vertex(new Vector3(0, -indicator_size * 0.375f * 0.5f, 0) * rot_ins + pos_pos, Color.Green);
                vtx[6].VtxOffs[l++] = m;
                vtx[6].Vtx[m++] = new Vertex(new Vector3(0, 0, indicator_size * 0.75f * 0.5f) * rot_ins + pos_pos, Color.Blue);
                vtx[6].Vtx[m++] = new Vertex(new Vector3(0, 0, -indicator_size * 0.375f * 0.5f) * rot_ins + pos_pos, Color.Blue);
                Color cur_color = Color.FromArgb(255, (int)(partData.LightsDirectional[i].Color_R * 255), (int)(partData.LightsDirectional[i].Color_G * 255), (int)(partData.LightsDirectional[i].Color_B * 255));  //Color.White;
                vtx[6].VtxOffs[l++] = m;
                for (int j = 0; j < circle_res; ++j)
                {
                    Vector3 vec = new Vector3(0, 0, LightSize);
                    vec *= Matrix3.Identity * Matrix3.CreateRotationX(MathHelper.TwoPi / circle_res * j);
                    vtx[6].Vtx[m++] = new Vertex(pos_pos + vec, cur_color);
                }
                vtx[6].VtxOffs[l++] = m;
                for (int j = 0; j < circle_res; ++j)
                {
                    Vector3 vec = new Vector3(0, 0, LightSize);
                    vec *= Matrix3.Identity * Matrix3.CreateRotationY(MathHelper.TwoPi / circle_res * j);
                    vtx[6].Vtx[m++] = new Vertex(pos_pos + vec, cur_color);
                }
                vtx[6].VtxOffs[l++] = m;
                for (int j = 0; j < circle_res; ++j)
                {
                    Vector3 vec = new Vector3(0, LightSize, 0);
                    vec *= Matrix3.Identity * Matrix3.CreateRotationZ(MathHelper.TwoPi / circle_res * j);
                    vtx[6].Vtx[m++] = new Vertex(pos_pos + vec, cur_color);
                }
                min_x = Math.Min(min_x, pos_pos.X);
                min_y = Math.Min(min_y, pos_pos.Y);
                min_z = Math.Min(min_z, pos_pos.Z);
                max_x = Math.Max(max_x, pos_pos.X);
                max_y = Math.Max(max_y, pos_pos.Y);
                max_z = Math.Max(max_z, pos_pos.Z);
            }
            for (int i = 0; i < partData.LightsPoint.Count; i++)
            {
                Matrix3 rot_ins = Matrix3.Identity;
                Vector3 pos_pos = partData.LightsPoint[i].Position.ToVec3();
                pos_pos.X = -pos_pos.X;
                float LightSize = partData.LightsPoint[i].Radius / 10f;
                vtx[6].VtxOffs[l++] = m;
                vtx[6].Vtx[m++] = new Vertex(new Vector3(-indicator_size * 0.75f * 0.5f, 0, 0) * rot_ins + pos_pos, Color.Red);
                vtx[6].Vtx[m++] = new Vertex(new Vector3(+indicator_size * 0.375f * 0.5f, 0, 0) * rot_ins + pos_pos, Color.Red);
                vtx[6].VtxOffs[l++] = m;
                vtx[6].Vtx[m++] = new Vertex(new Vector3(0, indicator_size * 0.75f * 0.5f, 0) * rot_ins + pos_pos, Color.Green);
                vtx[6].Vtx[m++] = new Vertex(new Vector3(0, -indicator_size * 0.375f * 0.5f, 0) * rot_ins + pos_pos, Color.Green);
                vtx[6].VtxOffs[l++] = m;
                vtx[6].Vtx[m++] = new Vertex(new Vector3(0, 0, indicator_size * 0.75f * 0.5f) * rot_ins + pos_pos, Color.Blue);
                vtx[6].Vtx[m++] = new Vertex(new Vector3(0, 0, -indicator_size * 0.375f * 0.5f) * rot_ins + pos_pos, Color.Blue);
                Color cur_color = Color.FromArgb(255, (int)(partData.LightsPoint[i].Color_R * 255), (int)(partData.LightsPoint[i].Color_G * 255), (int)(partData.LightsPoint[i].Color_B * 255));  //Color.Yellow;
                vtx[6].VtxOffs[l++] = m;
                for (int j = 0; j < circle_res; ++j)
                {
                    Vector3 vec = new Vector3(0, 0, LightSize);
                    vec *= Matrix3.Identity * Matrix3.CreateRotationX(MathHelper.TwoPi / circle_res * j);
                    vtx[6].Vtx[m++] = new Vertex(pos_pos + vec, cur_color);
                }
                vtx[6].VtxOffs[l++] = m;
                for (int j = 0; j < circle_res; ++j)
                {
                    Vector3 vec = new Vector3(0, 0, LightSize);
                    vec *= Matrix3.Identity * Matrix3.CreateRotationY(MathHelper.TwoPi / circle_res * j);
                    vtx[6].Vtx[m++] = new Vertex(pos_pos + vec, cur_color);
                }
                vtx[6].VtxOffs[l++] = m;
                for (int j = 0; j < circle_res; ++j)
                {
                    Vector3 vec = new Vector3(0, LightSize, 0);
                    vec *= Matrix3.Identity * Matrix3.CreateRotationZ(MathHelper.TwoPi / circle_res * j);
                    vtx[6].Vtx[m++] = new Vertex(pos_pos + vec, cur_color);
                }
                min_x = Math.Min(min_x, pos_pos.X);
                min_y = Math.Min(min_y, pos_pos.Y);
                min_z = Math.Min(min_z, pos_pos.Z);
                max_x = Math.Max(max_x, pos_pos.X);
                max_y = Math.Max(max_y, pos_pos.Y);
                max_z = Math.Max(max_z, pos_pos.Z);
            }
            for (int i = 0; i < partData.LightsNegative.Count; i++)
            {
                Matrix3 rot_ins = Matrix3.Identity;
                Vector3 pos_pos = partData.LightsNegative[i].Position.ToVec3();
                pos_pos.X = -pos_pos.X;
                float LightSize = partData.LightsNegative[i].Radius / 10f;
                vtx[6].VtxOffs[l++] = m;
                vtx[6].Vtx[m++] = new Vertex(new Vector3(-indicator_size * 0.75f * 0.5f, 0, 0) * rot_ins + pos_pos, Color.Red);
                vtx[6].Vtx[m++] = new Vertex(new Vector3(+indicator_size * 0.375f * 0.5f, 0, 0) * rot_ins + pos_pos, Color.Red);
                vtx[6].VtxOffs[l++] = m;
                vtx[6].Vtx[m++] = new Vertex(new Vector3(0, indicator_size * 0.75f * 0.5f, 0) * rot_ins + pos_pos, Color.Green);
                vtx[6].Vtx[m++] = new Vertex(new Vector3(0, -indicator_size * 0.375f * 0.5f, 0) * rot_ins + pos_pos, Color.Green);
                vtx[6].VtxOffs[l++] = m;
                vtx[6].Vtx[m++] = new Vertex(new Vector3(0, 0, indicator_size * 0.75f * 0.5f) * rot_ins + pos_pos, Color.Blue);
                vtx[6].Vtx[m++] = new Vertex(new Vector3(0, 0, -indicator_size * 0.375f * 0.5f) * rot_ins + pos_pos, Color.Blue);
                Color cur_color = Color.FromArgb(255, (int)(partData.LightsNegative[i].Color_R * 255), (int)(partData.LightsNegative[i].Color_G * 255), (int)(partData.LightsNegative[i].Color_B * 255));  //Color.White;
                vtx[6].VtxOffs[l++] = m;
                for (int j = 0; j < circle_res; ++j)
                {
                    Vector3 vec = new Vector3(0, 0, LightSize);
                    vec *= Matrix3.Identity * Matrix3.CreateRotationX(MathHelper.TwoPi / circle_res * j);
                    vtx[6].Vtx[m++] = new Vertex(pos_pos + vec, cur_color);
                }
                vtx[6].VtxOffs[l++] = m;
                for (int j = 0; j < circle_res; ++j)
                {
                    Vector3 vec = new Vector3(0, 0, LightSize);
                    vec *= Matrix3.Identity * Matrix3.CreateRotationY(MathHelper.TwoPi / circle_res * j);
                    vtx[6].Vtx[m++] = new Vertex(pos_pos + vec, cur_color);
                }
                vtx[6].VtxOffs[l++] = m;
                for (int j = 0; j < circle_res; ++j)
                {
                    Vector3 vec = new Vector3(0, LightSize, 0);
                    vec *= Matrix3.Identity * Matrix3.CreateRotationZ(MathHelper.TwoPi / circle_res * j);
                    vtx[6].Vtx[m++] = new Vertex(pos_pos + vec, cur_color);
                }
                min_x = Math.Min(min_x, pos_pos.X);
                min_y = Math.Min(min_y, pos_pos.Y);
                min_z = Math.Min(min_z, pos_pos.Z);
                max_x = Math.Max(max_x, pos_pos.X);
                max_y = Math.Max(max_y, pos_pos.Y);
                max_z = Math.Max(max_z, pos_pos.Z);
            }
            zFar = Math.Max(zFar, Math.Max(max_x - min_x, Math.Max(max_y - min_y, max_z - min_z)));
            UpdateVBO(6);
        }

        public void LoadAIPositions()
        {
            float min_x = float.MaxValue, min_y = float.MaxValue, min_z = float.MaxValue, max_x = float.MinValue, max_y = float.MinValue, max_z = float.MinValue;
            bool[] record_exists = new bool[8];
            int posi_count = 0;
            for (uint i = 0; i <= 7; ++i)
            {
                record_exists[i] = file.Data.ContainsItem(i);
                if (record_exists[i])
                {
                    if (file.Data.GetItem<TwinsSection>(i).ContainsItem(1))
                    {
                        posi_count += file.Data.GetItem<TwinsSection>(i).GetItem<TwinsSection>(1).Records.Count;
                        record_exists[i] = true;
                    }
                    else
                        record_exists[i] = false;
                }
            }
            if (vtx[4] == null || vtx.Length != (circle_res * 3 + 6) * posi_count)
            {
                vtx[4].VtxCounts = new int[6 * posi_count];
                vtx[4].VtxOffs = new int[6 * posi_count];
                vtx[4].Vtx = new Vertex[(circle_res * 3 + 6) * posi_count];
                for (int i = 0; i < posi_count; ++i)
                {
                    vtx[4].VtxCounts[i * 6 + 0] = 2;
                    vtx[4].VtxCounts[i * 6 + 1] = 2;
                    vtx[4].VtxCounts[i * 6 + 2] = 2;
                    vtx[4].VtxCounts[i * 6 + 3] = circle_res;
                    vtx[4].VtxCounts[i * 6 + 4] = circle_res;
                    vtx[4].VtxCounts[i * 6 + 5] = circle_res;
                }
            }
            int l = 0, m = 0;
            for (uint i = 0; i <= 7; ++i)
            {
                if (!record_exists[i]) continue;
                if (file.Data.GetItem<TwinsSection>(i).ContainsItem(1))
                {
                    foreach (AIPosition pos in file.Data.GetItem<TwinsSection>(i).GetItem<TwinsSection>(1).Records)
                    {
                        var ind_size = indicator_size * pos.Pos.W;
                        Vector3 pos_pos = pos.Pos.ToVec3();
                        pos_pos.X = -pos_pos.X;
                        vtx[4].VtxOffs[l++] = m;
                        vtx[4].Vtx[m++] = new Vertex(new Vector3(-ind_size * 0.75f * 0.5f, 0, 0) + pos_pos, Color.Red);
                        vtx[4].Vtx[m++] = new Vertex(new Vector3(+ind_size * 0.375f * 0.5f, 0, 0) + pos_pos, Color.Red);
                        vtx[4].VtxOffs[l++] = m;
                        vtx[4].Vtx[m++] = new Vertex(new Vector3(0, ind_size * 0.75f * 0.5f, 0) + pos_pos, Color.Green);
                        vtx[4].Vtx[m++] = new Vertex(new Vector3(0, -ind_size * 0.375f * 0.5f, 0) + pos_pos, Color.Green);
                        vtx[4].VtxOffs[l++] = m;
                        vtx[4].Vtx[m++] = new Vertex(new Vector3(0, 0, ind_size * 0.75f * 0.5f) + pos_pos, Color.Blue);
                        vtx[4].Vtx[m++] = new Vertex(new Vector3(0, 0, -ind_size * 0.375f * 0.5f) + pos_pos, Color.Blue);
                        Color cur_color = (file.SelectedItem == pos) ? Color.White : colors[colors.Length - i * 2 - 2];
                        vtx[4].VtxOffs[l++] = m;
                        for (int j = 0; j < circle_res; ++j)
                        {
                            Vector3 vec = new Vector3(0, 0, ind_size);
                            vec *= Matrix3.Identity * Matrix3.CreateRotationX(MathHelper.TwoPi / circle_res * j);
                            vtx[4].Vtx[m++] = new Vertex(pos_pos + vec, cur_color);
                        }
                        vtx[4].VtxOffs[l++] = m;
                        for (int j = 0; j < circle_res; ++j)
                        {
                            Vector3 vec = new Vector3(0, 0, ind_size);
                            vec *= Matrix3.Identity * Matrix3.CreateRotationY(MathHelper.TwoPi / circle_res * j);
                            vtx[4].Vtx[m++] = new Vertex(pos_pos + vec, cur_color);
                        }
                        vtx[4].VtxOffs[l++] = m;
                        for (int j = 0; j < circle_res; ++j)
                        {
                            Vector3 vec = new Vector3(0, ind_size, 0);
                            vec *= Matrix3.Identity * Matrix3.CreateRotationZ(MathHelper.TwoPi / circle_res * j);
                            vtx[4].Vtx[m++] = new Vertex(pos_pos + vec, cur_color);
                        }
                        min_x = Math.Min(min_x, pos_pos.X);
                        min_y = Math.Min(min_y, pos_pos.Y);
                        min_z = Math.Min(min_z, pos_pos.Z);
                        max_x = Math.Max(max_x, pos_pos.X);
                        max_y = Math.Max(max_y, pos_pos.Y);
                        max_z = Math.Max(max_z, pos_pos.Z);
                    }
                }
            }
            zFar = Math.Max(zFar, Math.Max(max_x - min_x, Math.Max(max_y - min_y, max_z - min_z)));
            UpdateVBO(4);
        }

        public void UpdateSelected()
        {
            if (file.SelectedItem is Instance ins)
            {
                SetPosition(new Vector3(-ins.Pos.X, ins.Pos.Y, ins.Pos.Z));
                LoadInstances();
            }
            else if (file.SelectedItem is Position pos)
            {
                SetPosition(new Vector3(-pos.Pos.X, pos.Pos.Y, pos.Pos.Z));
                LoadPositions();
            }
            else if (file.SelectedItem is Trigger trig)
            {
                SetPosition(new Vector3(-trig.Coords[1].X, trig.Coords[1].Y, trig.Coords[1].Z));
            }
            else if (file.SelectedItem is Camera cam)
            {
                SetPosition(new Vector3(-cam.TriggerPos.X, cam.TriggerPos.Y, cam.TriggerPos.Z));
            }
        }

        public void CustomTeleport(float X, float Y, float Z)
        {
            SetPosition(new Vector3(-X, Y, Z));
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}

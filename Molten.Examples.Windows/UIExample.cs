﻿using Molten.Data;
using Molten.Graphics;
using Molten.UI;

namespace Molten.Samples
{
    public class UIExample : SampleGame
    {
        ContentLoadHandle _hMaterial;
        ContentLoadHandle _hTexture;
        UIWindow _window1;
        UIWindow _window2;
        UILineGraph _lineGraph;
        UIButton _button1;
        UIButton _button2;

        GraphDataSet _graphSet;
        GraphDataSet _graphSet2;

        public override string Description => "Demonstrates Molten's UI system.";
        public UIExample() : base("UI Example") { }

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            _hMaterial = loader.Load<IMaterial>("assets/BasicTexture.mfx");
            _hTexture = loader.Load<ITexture2D>("assets/dds_test.dds", parameters: new TextureParameters()
            {
                GenerateMipmaps = true,
            });

            loader.Deserialize<UITheme>("assets/test_theme.json",(theme, isReload) =>
            {
                if (_window2 != null)
                    _window2.Theme = theme;

                if (_lineGraph != null)
                    _lineGraph.Theme = theme;
            });

            loader.OnCompleted += Loader_OnCompleted;
        }

        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            if (!_hMaterial.HasAsset())
            {
                Exit();
                return;
            }

            IMaterial mat = _hMaterial.Get<IMaterial>();
            ITexture2D texture = _hTexture.Get<ITexture2D>();
            
            mat.SetDefaultResource(texture, 0);
            TestMesh.Material = mat;

            _window1 = new UIWindow()
            {
                LocalBounds = new Rectangle(100, 150, 700, 440),
                Title = "This is a Window",
                //ShowDebugBounds = true,
            };

            _window2 = new UIWindow()
            {
                LocalBounds = new Rectangle(660, 250, 540, 550),
                Title = "This is another Window",
                //ShowDebugBounds = true
            };

            _button1 = new UIButton()
            {
                LocalBounds = new Rectangle(100, 100, 100, 30),
                Text = "Plot Data!"
            };

            _button2 = new UIButton()
            {
                LocalBounds = new Rectangle(100, 140, 120, 30),
                Text = "Plot More Data!"
            };

            _lineGraph = new UILineGraph()
            {
                LocalBounds = new Rectangle(0, 0, 700, 420)
            };

            PlotGraphData(_lineGraph);

            UI.Children.Add(_window1);
            UI.Children.Add(_window2);
            _window2.Children.Add(_button1);
            _window2.Children.Add(_button2);
            _window1.Children.Add(_lineGraph);

            _button1.Pressed += _button1_Pressed;
            _button2.Pressed += _button2_Pressed;
        }

        private void _button1_Pressed(UIElement element, ScenePointerTracker tracker)
        {
            _graphSet.Plot(Rng.Next(10, 450));
        }

        private void _button2_Pressed(UIElement element, ScenePointerTracker tracker)
        {
            _graphSet2.Plot(Rng.Next(100, 300));
        }

        private void PlotGraphData(UILineGraph graph)
        {
            _graphSet = new GraphDataSet(200);
            _graphSet.KeyColor = Color.Grey;
            for (int i = 0; i < _graphSet.Capacity; i++)
                _graphSet.Plot(Rng.Next(0, 500));

            _graphSet2 = new GraphDataSet(200);
            _graphSet2.KeyColor = Color.Lime;
            float piInc = MathHelper.TwoPi / 20;
            float waveScale = 100;
            for (int i = 0; i < _graphSet2.Capacity; i++)
                _graphSet2.Plot(waveScale * Math.Sin(piInc * i));

            graph.AddDataSet(_graphSet);
            graph.AddDataSet(_graphSet2);
        }

        protected override void OnDrawSprites(SpriteBatcher sb)
        {
            if (SampleFont == null)
                return;

            base.OnDrawSprites(sb);

            string text = $"Hovered UI Element: {(UI.HoverElement != null ? UI.HoverElement.Name : "None")}";
            Vector2F tSize = SampleFont.MeasureString(text);
            Vector2F pos = new Vector2F()
            {
                X = (Window.Width / 2) - (tSize.X / 2),
                Y = 25,
            };

            sb.DrawString(SampleFont, text, pos, Color.White);
        }
    }
}

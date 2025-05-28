using Silk.NET.SDL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System.IO;
using Silk.NET.Maths;

namespace TheAdventure;

public unsafe class GameRenderer
{
    public readonly struct TextureInfo
    {
        public int Width { get; init; }
        public int Height { get; init; }
        public int PixelDataSize => Width * Height * 4;
    }

    private readonly Sdl _sdl;
    private readonly IntPtr _renderer;
    private readonly GameLogic _gameLogic;

    private readonly Dictionary<int, IntPtr> _texturePointers;
    private readonly Dictionary<int, TextureInfo> _textureInformation;
    private int _index = 0;

    public GameRenderer(Sdl sdl, GameWindow gameWindow, GameLogic gameLogic)
    {
        _sdl = sdl;
        _renderer = gameWindow.CreateRenderer();
        _gameLogic = gameLogic;
        _textureInformation = new();
        _texturePointers = new();
    }

    public void Render()
    {
        var renderer = (Renderer*)_renderer;

        _sdl.RenderClear(renderer);

        foreach (var renderable in _gameLogic.GetRenderables())
        {
            if (renderable.TextureId > -1 &&
                _texturePointers.TryGetValue(renderable.TextureId, out var texturePointer))
            {
                Silk.NET.SDL.Point center = new Silk.NET.SDL.Point { X = 0, Y = 0 };
                _sdl.RenderCopyEx(renderer, (Texture*)texturePointer, renderable.TextureSource,
                    renderable.TextureDestination, 0, &center, RendererFlip.None);
            }
        }

        DrawScore(renderer, _gameLogic.Score);

        _sdl.RenderPresent(renderer);
    }

    private void DrawScore(Renderer* renderer, int score)
    {
        string text = $"Scor: {score}";
        var font = SystemFonts.CreateFont("Arial", 24);
        var img = new Image<Rgba32>(200, 50);
        img.Mutate(x =>
        {
            x.Fill(SixLabors.ImageSharp.Color.Transparent);
            x.DrawText(text, font, SixLabors.ImageSharp.Color.White, new PointF(0, 0));
        });

        var pixels = new byte[img.Width * img.Height * 4];
        img.CopyPixelDataTo(pixels);

        fixed (byte* data = pixels)
        {
            var surface = _sdl.CreateRGBSurfaceWithFormatFrom(data, img.Width, img.Height, 32, img.Width * 4,
                (uint)PixelFormatEnum.Abgr8888);
            var texture = _sdl.CreateTextureFromSurface((Renderer*)_renderer, surface);
            _sdl.FreeSurface(surface);

            var dst = new Rectangle<int>(10, 10, img.Width, img.Height);
            _sdl.RenderCopy((Renderer*)_renderer, texture, null, &dst);
            _sdl.DestroyTexture(texture);
        }
    }

    public int LoadTexture(string fileName, out TextureInfo textureInfo)
    {
        using var fStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        var image = Image.Load<Rgba32>(fStream);
        textureInfo = new TextureInfo
        {
            Width = image.Width,
            Height = image.Height
        };
        var imageRawData = new byte[textureInfo.PixelDataSize];
        image.CopyPixelDataTo(imageRawData.AsSpan());
        Texture* imageTexture = null;
        fixed (byte* data = imageRawData)
        {
            var surface = _sdl.CreateRGBSurfaceWithFormatFrom(data, textureInfo.Width, textureInfo.Height, 8,
                textureInfo.Width * 4, (uint)PixelFormatEnum.Rgba32);
            imageTexture = _sdl.CreateTextureFromSurface((Renderer*)_renderer, surface);
            _sdl.FreeSurface(surface);
        }

        if (imageTexture == null) return -1;

        _texturePointers[_index] = (IntPtr)imageTexture;
        _textureInformation[_index] = textureInfo;
        return _index++;
    }
}

using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using _3DTools;
using Ab3d;
using GetSTEM.Model3DBrowser.Services;
using Petzold.Media3D;

namespace GetSTEM.Model3DBrowser.ViewModels
{
    public class ModelLoader
    {
        public static ModelResult LoadModel3DGroup(IConfigurationService configService, bool designMode)
        {
            if (designMode)
            {
                var mesh = new CubeMesh();
                var model = new GeometryModel3D(mesh.Geometry,
                     new DiffuseMaterial(new SolidColorBrush(Colors.Red)));
                var group = new Transform3DGroup();
                group.Children.Add(new ScaleTransform3D(new Vector3D(20, 20, 20)));
                group.Children.Add(new RotateTransform3D(
                    new AxisAngleRotation3D(new Vector3D(1, 1, 0), 30)));
                model.Transform = group;
                var designSesult = new ModelResult();
                designSesult.Model3DGroup.Children.Add(model);

                return designSesult;
            }

            var result = new ModelResult();
            var model3DGroup = new Model3DGroup();
            var modelConfig = configService.GetModelConfiguration();

            foreach (var letter in modelConfig.LetterModels)
            {
                var reader = new Reader3ds();
                var thing = reader.ReadFile(letter.ModelPath);
                var internalModel = GetModelFromGroup(thing);

                if (internalModel == null)
                {
                    throw new InvalidOperationException("3D Model could not be found in 3DS file '" + letter.ModelPath + "'");
                }

                var mesh = (MeshGeometry3D)internalModel.Geometry;

                var brush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(letter.Color));
                var materialGroup = new MaterialGroup();
                var diffuse = new DiffuseMaterial(brush);
                var specular = new SpecularMaterial(new SolidColorBrush(Colors.White), 1000);
                materialGroup.Children.Add(diffuse);
                materialGroup.Children.Add(specular);

                var model = new GeometryModel3D(mesh, materialGroup);

                var transformGroup = new Transform3DGroup();

                var scale = new ScaleTransform3D(
                    new Vector3D(letter.Scale, letter.Scale, letter.Scale));

                var translate = new TranslateTransform3D(
                    new Vector3D(letter.OffsetX, letter.OffsetY, letter.OffsetZ));

                transformGroup.Children.Add(translate);
                transformGroup.Children.Add(scale);

                model.Transform = transformGroup;

                model3DGroup.Children.Add(model);

                var wireframe = new ScreenLines();

                for (var i = 0; i < mesh.Positions.Count; i++)
                {
                    wireframe.Points.Add(mesh.Positions[i]);
                    wireframe.Thickness = 1;
                    wireframe.Color = Colors.White;
                }

                wireframe.Transform = transformGroup;
                result.Wireframes.Add(wireframe);
            }

            result.Model3DGroup = model3DGroup;
            return result;
        }

        static GeometryModel3D GetModelFromGroup(Model3DGroup group)
        {
            foreach (var child in group.Children)
            {
                if (child is GeometryModel3D)
                {
                    return (GeometryModel3D)child;
                }

                if (child is Model3DGroup)
                {
                    var result = GetModelFromGroup((Model3DGroup)child);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

    }
}

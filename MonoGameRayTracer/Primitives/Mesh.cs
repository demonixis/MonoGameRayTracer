using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameRayTracer.DataStructure;
using MonoGameRayTracer.Materials;
using MonoGameRayTracer.Utils;

namespace MonoGameRayTracer.Primitives
{
    public class Mesh : Hitable
    {
        public struct Triangle
        {
            public Vector3 V0;
            public Vector3 V1;
            public Vector3 V2;

            public Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
            {
                V0 = v0;
                V1 = v1;
                V2 = v2;
            }
        }

        public struct TriangleIndice
        {
            public int A, B, C;
        }

        private List<Triangle> m_Triangles;

        public Mesh(Model model, Material material)
        {
            m_Material = material;

            var vertices = new List<Vector3>();
            var indices = new List<TriangleIndice>();
            var transform = Matrix.CreateScale(0.001f);

            foreach (var m in model.Meshes)
            {
                foreach (var part in m.MeshParts)
                {
                    ExtractModelMeshPartData(part, ref transform, ref vertices, ref indices);
                }
            }

            m_Triangles = new List<Triangle>();

            foreach (var indice in indices)
            {
                m_Triangles.Add(new Triangle(
                    vertices[indice.A],
                    vertices[indice.B],
                    vertices[indice.C]
                ));
            }

            Console.WriteLine($"Extracted {m_Triangles.Count} triangles");
        }

        public override bool BoundingBox(ref AABB box)
        {
            box = m_BoundingBox;
            return true;
        }

        public override bool Hit(ref Ray ray, float min, float max, ref HitRecord record)
        {
            Triangle triangle;

            for (var i = 0; i < m_Triangles.Count; i++)
            {
                triangle = m_Triangles[i];
                var hit = Hit(ref ray, ref triangle.V0, ref triangle.V1, ref triangle.V2, ref record, false);
                if (hit)
                {
                    record.Material = m_Material;
                    return true;
                }
            }

            return false;
        }

        // MollerTrumber
        public static bool Hit(ref Ray ray, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2, ref HitRecord record, bool culling)
        {
            var v0v1 = v1 - v0;
            var v0v2 = v2 - v0;
            var pvec = Vector3.Cross(ray.Direction, v0v2);
            var det = Vector3.Dot(v0v1, pvec);

            // If the determinant is negative the triangle is backfacing
            // If the determinant is close to 0, the ray misses the triangle.
            if (culling && det < Mathf.Epsilon)
                return false;

            // Ray and triangle are parallel if det is close to 0.
            if (Mathf.Abs(det) < Mathf.Epsilon)
                return false;

            var invDet = 1.0f / det;
            var tvec = ray.Origin - v0;

            var u = Vector3.Dot(tvec, pvec) * invDet;

            if (u < 0 || u > 1)
                return false;

            var qvec = Vector3.Cross(tvec, v0v1);
            var v = Vector3.Dot(ray.Direction, qvec) * invDet;

            if (v < 0 || u + v > 1)
                return false;

            record.T = Vector3.Dot(v0v1, qvec) * invDet;
            record.P = ray.PointAtParameter(record.T);
            record.U = u;
            record.V = v;

            return true;
        }

        public static void ExtractModelMeshPartData(ModelMeshPart meshPart, ref Matrix transform, ref List<Vector3> vertices, ref List<TriangleIndice> indices)
        {
            var offset = vertices.Count;
            var declaration = meshPart.VertexBuffer.VertexDeclaration;
            var vertexElements = declaration.GetVertexElements();
            var vertexPosition = new VertexElement();

            foreach (var vert in vertexElements)
            {
                if (vert.VertexElementUsage == VertexElementUsage.Position && vert.VertexElementFormat == VertexElementFormat.Vector3)
                {
                    vertexPosition = vert;
                    break;
                }
            }

            var allVertex = new Vector3[meshPart.NumVertices];
            meshPart.VertexBuffer.GetData<Vector3>(meshPart.VertexOffset * declaration.VertexStride + vertexPosition.Offset, allVertex, 0, meshPart.NumVertices, declaration.VertexStride);

            for (var i = 0; i != allVertex.Length; ++i)
                Vector3.Transform(ref allVertex[i], ref transform, out allVertex[i]);

            vertices.AddRange(allVertex);

            var indexElements = new short[meshPart.PrimitiveCount * 3];
            meshPart.IndexBuffer.GetData<short>(meshPart.StartIndex * 2, indexElements, 0, meshPart.PrimitiveCount * 3);

            var tvi = new TriangleIndice[meshPart.PrimitiveCount];
            for (int i = 0; i != tvi.Length; ++i)
            {
                tvi[i].A = indexElements[i * 3 + 0] + offset;
                tvi[i].B = indexElements[i * 3 + 1] + offset;
                tvi[i].C = indexElements[i * 3 + 2] + offset;
            }

            indices.AddRange(tvi);
        }
    }
}

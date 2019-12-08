﻿using OpenTK;
using System;

namespace Minecraft
{
    class ViewFrustum
    {
        //Implementation: https://cgvr.cs.uni-bremen.de/teaching/cg_literatur/lighthouse3d_view_frustum_culling/index.html
        struct FrustumPlane
        {
            //public Vector3 point;
            public Vector3 normal;
            public float distanceToOrigin;

            public float GetSignedDistance(Vector3 point)
            {
                return Vector3.Dot(point, normal) - distanceToOrigin;
            }
        }

        private float fov;
        private float aspectRatio;
        private float nearDistance;
        private float farDistance;
        private float nearWidth, nearHeight, farWidth, farHeight;

        private FrustumPlane[] frustumPlanes = new FrustumPlane[6];

        public ViewFrustum(float fov, float aspectRatio, float nearDistance, float farDistance)
        {
            this.fov = fov;
            this.aspectRatio = aspectRatio;
            this.nearDistance = nearDistance;
            this.farDistance = farDistance;
            CalculateNearFarWidthHeight();
        }

        private void CalculateNearFarWidthHeight()
        {
            float extension = 2;
            float tan = (float)Math.Tan(fov * 0.5) * extension;
            nearHeight = tan * nearDistance;
            nearWidth = nearHeight * aspectRatio;
            farHeight = tan * farDistance;
            farWidth = farHeight * aspectRatio;
        }

        /*public Matrix4 CreateProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(fov, aspectRatio, nearDistance, farDistance);
        }*/

        public void UpdateFrustumPoints(Camera camera)
        {
            Vector3 zAxis = -camera.forward;
            Vector3 xAxis = camera.right;
            Vector3 yAxis = Vector3.Cross(zAxis, xAxis);

            Vector3 nearCenter = camera.position - zAxis * nearDistance;
            Vector3 farCenter = camera.position - zAxis * farDistance;

            frustumPlanes[0].normal = -zAxis; //near plane
            frustumPlanes[0].distanceToOrigin = Vector3.Dot(frustumPlanes[0].normal, nearCenter);

            frustumPlanes[1].normal = zAxis; //far plane
            frustumPlanes[1].distanceToOrigin = Vector3.Dot(frustumPlanes[1].normal, farCenter);

            Vector3 temporary, normal;

            temporary = (nearCenter + yAxis * nearHeight) - camera.position;
            temporary.Normalize();
            normal = Vector3.Cross(temporary, xAxis);
            frustumPlanes[2].normal = normal; //top plane
            frustumPlanes[2].distanceToOrigin = Vector3.Dot(frustumPlanes[2].normal, nearCenter + yAxis * nearHeight);

            temporary = (nearCenter - yAxis * nearHeight) - camera.position;
            temporary.Normalize();
            normal = Vector3.Cross(xAxis, temporary);
            frustumPlanes[3].normal = normal; //bottom plane
            frustumPlanes[3].distanceToOrigin = Vector3.Dot(frustumPlanes[3].normal, nearCenter - yAxis * nearHeight);

            temporary = (nearCenter - xAxis * nearWidth) - camera.position;
            temporary.Normalize();
            normal = Vector3.Cross(temporary, yAxis);
            frustumPlanes[4].normal = normal; //left plane
            frustumPlanes[4].distanceToOrigin = Vector3.Dot(frustumPlanes[4].normal, nearCenter - xAxis * nearWidth);

            temporary = (nearCenter + xAxis * nearWidth) - camera.position;
            temporary.Normalize();
            normal = Vector3.Cross(yAxis, temporary);
            frustumPlanes[5].normal = normal; //right plane
            frustumPlanes[5].distanceToOrigin = Vector3.Dot(frustumPlanes[5].normal, nearCenter + xAxis * nearWidth);
        }

        public bool IsSphereInFrustum(Vector3 position, float radius)
        {
            for (int i = 0; i < 6; i++)
            {
                if (frustumPlanes[i].GetSignedDistance(position) < -radius)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsAABBInFrustum(AABB aabb)
        {
            Vector3[] corners = aabb.GetAllCorners();
            for (int i = 0; i < 6; i++)
            {
                int inside = 0;
                foreach (Vector3 corner in corners)
                {
                    if (frustumPlanes[i].GetSignedDistance(corner) >= 0)
                    {
                        inside++;
                    }
                }
                if (inside == 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
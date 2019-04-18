using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace EngineCore.ECS.Components
{
    public class Transform : IEntityComponent//, IEntityComponentWithEntityId
    {
        /// <summary>
        /// Indicates when transform must be updated.
        /// </summary>
        internal bool IsNeedUpdate = true;

        /// <summary>
        /// Transform position vector.
        /// </summary>
        private Vector3 m_Position = Vector3.Zero;
        /// <summary>
        /// Gets or sets transform position vector.
        /// </summary>
        public Vector3 Position {
            get {
                return m_Position;
            }
            set {
                if (m_Position != value)
                {
                    IsNeedUpdate = true;
                }
                m_Position = value;
            }
        }

        /// <summary>
        /// Transform rotation quaternion.
        /// </summary>
        private Quaternion m_Rotation = Quaternion.Identity;
        /// <summary>
        /// Gets or sets transform rotation quaternion.
        /// </summary>
        public Quaternion Rotation {
            get {
                return m_Rotation;
            }
            set {
                if (m_Rotation != value)
                {
                    IsNeedUpdate = true;
                }
                m_Rotation = value;
            }
        }

        /// <summary>
        /// Transform scale vector.
        /// </summary>
        private Vector3 m_Scale = Vector3.One;
        /// <summary>
        /// Gets or sets transform scale vector.
        /// </summary>
        public Vector3 Scale {
            get {
                return m_Scale;
            }
            set {
                if (m_Scale != value)
                {
                    IsNeedUpdate = true;
                }
                m_Scale = value;
            }
        }

        /// <summary>
        /// Fast transformations setter.
        /// </summary>
        /// <param name="position">Position vector.</param>
        /// <param name="rotation">Rotation quaternion.</param>
        public void SetTransformations(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
        /// <summary>
        /// Fast transformations setter.
        /// </summary>
        /// <param name="position">Position vector.</param>
        /// <param name="rotation">Rotation quaternion.</param>
        /// <param name="scale">Scale vector.</param>
        public void SetTransformations(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        /// <summary>
        /// Get transform world matrix.
        /// </summary>
        public Matrix TransformMatrix { get; internal set; }

        /// <summary>
        /// Get previous transform world matrix.
        /// </summary>
        public Matrix PreviousTransformMatrix { get; internal set; }

        /// <summary>
        /// Get transform direction vector.
        /// </summary>
        public Vector3 Direction {
            get {
                return new Vector3(2 * (Rotation.X * Rotation.Z + Rotation.W * Rotation.Y),
                    2 * (Rotation.Y * Rotation.Z - Rotation.W * Rotation.X),
                    1 - 2 * (Rotation.X * Rotation.X + Rotation.Y * Rotation.Y));
            }
        }
        //public int EntityId { get; set; }

        /*
        /// <summary>
        /// Constructor
        /// </summary>
        public TransformComponent()
        {
            if (m_RootTransform == null) {
                return;
            }
            Parent = m_RootTransform;
        }
        #region Hierarchy

        /// <summary>
        /// Static Root TransformComponent for world (actually for all project).
        /// </summary>
        static private TransformComponent m_RootTransform;
        static private TransformComponent RootTransform {
            get {
                if (m_RootTransform == null)
                {
                    m_RootTransform = new TransformComponent();
                }
                return m_RootTransform;
            }
        }

        /// <summary>
        /// The number of children the parent TransformComponent has.
        /// <note type="note">
        /// The parent is not included in the count.
        /// </note>
        /// </summary>
        public int childCount {get; private set;}

        /// <summary>
        /// Internal value for detect hierarchy level of TransformComponent.
        /// </summary>
        private int hierarchyLevel = 0;

        /// <summary>
        /// The parent of the TransformComponent.
        /// </summary>
        public TransformComponent Parent;

        /// <summary>
        /// First child TransformComponent reference.
        /// </summary>
        private TransformComponent FirstChild;

        /// <summary>
        /// Previous sibling TransformComponent reference.
        /// </summary>
        private TransformComponent PrevSibling;

        /// <summary>
        /// Next sibling TransformComponent reference.
        /// </summary>
        private TransformComponent NextSibling;

        /// <summary>
        /// Set the parent of the transform.
        /// </summary>
        /// <param name="parent">The parent TransformComponent to use.</param>
        public void SetParent(TransformComponent parent)
        {
            //TODO: check if it posible
            DetachFromParent();
            AttachToParent(parent ?? RootTransform);
        }

        /// <summary>
        /// Attach transform to new parent.
        /// </summary>
        /// <param name="parent">The parent TransformComponent to use.</param>
        private void AttachToParent(TransformComponent parent)
        {
            Parent = parent;
            if (Parent.FirstChild != null)
            {
                Parent.FirstChild.PrevSibling = this;
                NextSibling = Parent.FirstChild;
            }
            this.Parent.FirstChild = this;
            this.Parent.childCount++;
            hierarchyLevel = parent.hierarchyLevel + 1;
        }

        /// <summary>
        /// Move the transform to the start of the transform child list.
        /// </summary>
        public void SetAsFirstSibling()
        {
            ShiftSibling();
            PrevSibling = null;
            Parent.FirstChild.PrevSibling = this;
            NextSibling = Parent.FirstChild;
            Parent.FirstChild = this;
        }

        /// <summary>
        /// Move the transform to the end of the transform child list.
        /// </summary>
        public void SetAsLastSibling()
        {
            ShiftSibling();
            NextSibling = null;
            TransformComponent LastSibling = Parent.FirstChild;
            while (LastSibling.NextSibling != null)
            {
                LastSibling = LastSibling.NextSibling;
            }
            LastSibling.NextSibling = this;
            PrevSibling = LastSibling;
        }

        /// <summary>
        /// Unparent this transform.
        /// </summary>
        public void DetachFromParent()
        {
            if (this.Parent != null)
            {
                if (this.Parent.FirstChild == this)
                {
                    this.Parent.FirstChild = this.NextSibling;
                }
                this.Parent.childCount--;
            }

            ShiftSibling();

            this.hierarchyLevel = 0;
            this.Parent = null;
            this.NextSibling = null;
            this.PrevSibling = null;
        }

        /// <summary>
        /// Shift sibling for detach or move sibling order.
        /// </summary>
        private void ShiftSibling()
        {
            if (PrevSibling != null)
            {
                PrevSibling.NextSibling = NextSibling;
            }
            if (NextSibling != null)
            {
                NextSibling.PrevSibling = PrevSibling;
            }
        }

        /// <summary>
        /// Unparents all children.
        /// </summary>
        public void DetachChildren()
        {
            TransformComponent next = FirstChild;
            while (next != null)
            {
                next.AttachToParent(RootTransform);
                next = next.NextSibling;
            }
            childCount = 0;
        }

        /// <summary>
        /// Returns a TransformComponent child by index.
        /// </summary>
        /// <param name="index">Index of the child TransformComponent to return. Must be smaller than TransformComponent.childCount.</param>
        /// <returns><c>TransformComponent</c> TransformComponent child by index.</returns>
        public TransformComponent GetChild(int index)
        {
            if (index > childCount - 1) {
                //TODO: warning.
                return null;
            }

            if (index == 0) {
                return FirstChild;
            }

            TransformComponent Child = FirstChild;
            for (int i = 0; i < index; i++)
            {
                Child = Child.NextSibling;
            }

            return FirstChild;
        }

        /// <summary>
        /// Is this TransformComponent a child of parent?
        /// </summary>
        /// <param name="parent"></param>
        /// <returns>A boolean value that indicates whether the TransformComponent is a child of a given transform. 
        /// <c>true</c> if this TransformComponent is a child, deep child (child of a child) or 
        /// identical to this TransformComponent, otherwise <c>false</c>.</returns>
        public bool IsChildOf(TransformComponent parent)
        {
            if (parent.hierarchyLevel >= hierarchyLevel)
            {
                return false;
            }
            TransformComponent it = Parent;
            while (it != parent || parent.hierarchyLevel >= Parent.hierarchyLevel)
            {
                it = it.Parent;
            }
            return it == parent;
        }

        #endregion

        #region Transform
        //TODO: Matricies and their stuff.
        #endregion
        */
    }
}

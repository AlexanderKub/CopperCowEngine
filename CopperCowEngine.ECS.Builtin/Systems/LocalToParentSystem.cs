using System.Collections.Generic;
using CopperCowEngine.ECS.Builtin.Components;

namespace CopperCowEngine.ECS.Builtin.Systems
{
    public class LocalToParentSystem : ComponentSystem<Required<LocalToWorld, LocalToParent, Parent>>
    {
        private readonly Stack<Entity> _parentsStack = new Stack<Entity>();

        private readonly List<int> _updated = new List<int>();

        protected override void Update()
        {
            _updated.Clear();
            
            foreach (var slice in Iterator)
            {
                if (_updated.Contains(slice.Entity.Id))
                {
                    continue;
                }

                ref var locToWorld = ref slice.Sibling<LocalToWorld>();

                var locToParent = slice.Sibling<LocalToParent>();
                
                var parentEntity = slice.Sibling<Parent>().Value;

                Process(parentEntity);

                var parentLocToWorld = Context.GetComponent<LocalToWorld>(parentEntity);

                locToWorld.Value = locToParent.Value * parentLocToWorld.Value;

                _updated.Add(slice.Entity.Id);
            }
        }

        private void Process(Entity entity)
        {
            if (!Context.HasComponent<Parent>(entity) || _updated.Contains(entity.Id))
            {
                return;
            }

            _parentsStack.Push(entity);

            var parentEntity = Context.GetComponent<Parent>(entity).Value;
            while (Context.HasComponent<Parent>(parentEntity) && !_updated.Contains(parentEntity.Id))
            {
                _parentsStack.Push(parentEntity);
                parentEntity = Context.GetComponent<Parent>(entity).Value;
            }

            while (_parentsStack.Count > 0)
            {
                entity = _parentsStack.Pop();

                ref var locToWorld = ref Context.GetComponent<LocalToWorld>(entity);
                var locToParent = Context.GetComponent<LocalToParent>(entity);

                var parentLocToWorld = Context.GetComponent<LocalToWorld>(_parentsStack.Count > 0 ? 
                    _parentsStack.Peek() : Context.GetComponent<Parent>(entity).Value);

                locToWorld.Value = locToParent.Value * parentLocToWorld.Value;

                _updated.Add(entity.Id);
            }
        }
    }
}

/*using Microsoft.EntityFrameworkCore;
using Order;

namespace Order.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Удаление пользователя с отвязкой от него 
        public async Task RemoveTask(int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                throw new ArgumentException("Task not found");

            var project = await _context.Projects.FindAsync(task.ProjectId);

            if (project != null)
            {
                project.TaskIds.Remove(taskId); // Удаляем ID задачи из списка TaskIds
                _context.Entry(project).State = EntityState.Modified;
            }

            var evt = await _context.Events.FindAsync(task.EventId);

            if (evt != null)
            {
                evt.TaskIds.Remove(taskId); // Удаляем ID задачи из списка TaskIds
                _context.Entry(evt).State = EntityState.Modified;
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }
    }
}
*/
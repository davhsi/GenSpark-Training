export interface TaskItem {
  id: number;
  title: string;
  description: string;
  isCompleted: boolean;
  createdAt: string;
}

export type CreateTaskItem = Omit<TaskItem, 'id' | 'createdAt'>;
export type UpdateTaskItem = TaskItem;

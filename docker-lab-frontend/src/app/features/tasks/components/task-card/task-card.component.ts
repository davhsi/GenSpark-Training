import { Component, input, output } from '@angular/core';
import { DatePipe, NgClass, DecimalPipe } from '@angular/common';
import { TaskItem } from '../../models/task.model';

@Component({
  selector: 'app-task-card',
  standalone: true,
  imports: [DatePipe, NgClass, DecimalPipe],
  template: `
    <div class="group relative overflow-hidden bg-zinc-900 border border-zinc-800 p-6 rounded-none transition-all duration-300 hover:border-emerald-500/50 flex flex-col justify-between h-full"
         [ngClass]="{'opacity-60 grayscale': task().isCompleted}">
      <!-- Liquid Glass Inner Border -->
      <div class="absolute inset-0 border border-white/5 pointer-events-none"></div>

      <div>
        <div class="flex items-start justify-between mb-4">
          <div>
            <h3 class="font-sans text-xl font-medium tracking-tight text-white mb-1"
                [ngClass]="{'line-through text-zinc-500': task().isCompleted}">
              {{ task().title }}
            </h3>
            <p class="font-mono text-xs text-zinc-500">
              ID: {{ task().id | number:'3.0-0' }} // {{ task().createdAt | date:'short' }}
            </p>
          </div>
          <button 
            (click)="toggleStatus.emit(task())"
            class="w-6 h-6 border transition-colors flex items-center justify-center shrink-0 cursor-pointer"
            [ngClass]="task().isCompleted ? 'bg-emerald-500 border-emerald-500' : 'bg-transparent border-zinc-700 hover:border-emerald-500'">
            @if (task().isCompleted) {
              <svg class="w-4 h-4 text-zinc-950" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="square" stroke-linejoin="miter" stroke-width="2" d="M5 13l4 4L19 7"></path></svg>
            }
          </button>
        </div>

        <p class="font-sans text-sm text-zinc-400 leading-relaxed"
           [ngClass]="{'line-through': task().isCompleted}">
          {{ task().description || 'No description provided.' }}
        </p>
      </div>

      <div class="mt-6 flex justify-end">
        <button 
          (click)="delete.emit(task().id)"
          class="font-mono text-xs text-zinc-600 hover:text-red-500 transition-colors uppercase tracking-widest cursor-pointer">
          [ Delete ]
        </button>
      </div>
    </div>
  `
})
export class TaskCardComponent {
  task = input.required<TaskItem>();
  toggleStatus = output<TaskItem>();
  delete = output<number>();
}

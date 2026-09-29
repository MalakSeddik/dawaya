import type { ComponentProps } from 'react';

import { cn } from '@/lib/utils';

// shadcn/ui input, sized for touch (44px) and readable text.
export function Input({ className, type = 'text', ...props }: ComponentProps<'input'>) {
  return (
    <input
      type={type}
      data-slot="input"
      className={cn(
        'min-h-touch w-full rounded-md border-(length:--border-width) bg-card px-3 py-2 text-base placeholder:text-muted-foreground disabled:opacity-50 aria-invalid:border-destructive',
        className,
      )}
      {...props}
    />
  );
}

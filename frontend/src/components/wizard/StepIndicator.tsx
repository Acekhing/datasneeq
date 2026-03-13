'use client';

import { cn } from '@/lib/utils';
import { Check } from 'lucide-react';

interface Step {
  label: string;
  description: string;
}

const steps: Step[] = [
  { label: 'Upload', description: 'Select Excel file' },
  { label: 'Connect', description: 'Database & table' },
  { label: 'Map', description: 'Column mapping' },
  { label: 'Preview', description: 'Validate data' },
  { label: 'Upload', description: 'Confirm & insert' },
];

interface StepIndicatorProps {
  currentStep: number;
}

export default function StepIndicator({ currentStep }: StepIndicatorProps) {
  return (
    <div className="flex items-center justify-between mb-8">
      {steps.map((step, index) => (
        <div key={step.label + index} className="flex items-center flex-1">
          <div className="flex flex-col items-center">
            <div
              className={cn(
                'w-10 h-10 rounded-full flex items-center justify-center text-sm font-medium border-2 transition-all',
                index < currentStep
                  ? 'bg-primary text-primary-foreground border-primary'
                  : index === currentStep
                    ? 'border-primary text-primary bg-primary/10'
                    : 'border-muted-foreground/30 text-muted-foreground'
              )}
            >
              {index < currentStep ? <Check className="w-5 h-5" /> : index + 1}
            </div>
            <span
              className={cn(
                'text-xs mt-1 font-medium',
                index <= currentStep ? 'text-primary' : 'text-muted-foreground'
              )}
            >
              {step.label}
            </span>
            <span className="text-[10px] text-muted-foreground">{step.description}</span>
          </div>
          {index < steps.length - 1 && (
            <div
              className={cn(
                'flex-1 h-0.5 mx-2 mt-[-18px]',
                index < currentStep ? 'bg-primary' : 'bg-muted-foreground/30'
              )}
            />
          )}
        </div>
      ))}
    </div>
  );
}

import { updates } from '../data/changelog';

const formatDate = (date: string) =>
  new Date(date).toLocaleDateString(undefined, {
    timeZone: 'UTC',
    month: 'short',
    day: 'numeric',
    year: 'numeric'
  });

export default function ChangeLog() {
  return (
    <div className='flex flex-col divide-y divide-secondary/20'>
      {updates.map((update) => (
        <section key={update.date} className='py-5 first:pt-0 last:pb-0'>
          <h3 className='mb-3 text-xs font-medium uppercase tracking-wider text-secondary'>
            <time dateTime={update.date}>{formatDate(update.date)}</time>
          </h3>
          <div className='space-y-4'>
            {update.changes.map((change) => (
              <div key={change.title}>
                <h4 className='font-semibold text-primary'>{change.title}</h4>
                <ul className='mt-1.5 list-disc space-y-1 pl-5 text-sm leading-relaxed text-primary/80 marker:text-secondary'>
                  {change.items.map((item) => (
                    <li key={item}>{item}</li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        </section>
      ))}
    </div>
  );
}

interface Update {
  date: string; // YYYY-MM-DD
  changes: { title: string; items: string[] }[];
}

export const updates: Update[] = [
  // newest first
  {
    date: '2026-10-12',
    changes: [
      {
        title: 'Cleaner line names',
        items: [
          'Number-only lines now show as "Line 5" instead of "5"',
          'Number-only lines now show as "Line 5" instead of "5"',
          'Line names no longer include years of operation, e.g. (2015-2017)'
        ]
      },
      {
        title: 'Altered compass direction',
        items: ['Direction hints now snap to the nearest 45° compass point (N, NE, E…) instead of an exact angle']
      }
    ]
  }
];

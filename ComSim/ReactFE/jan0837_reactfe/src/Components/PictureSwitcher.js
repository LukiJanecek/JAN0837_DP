import { useState } from 'react';
import { Button } from 'react-bootstrap';
import Picture from './Picture';

function PictureSwitcher({names = [], ext = 'png', folder = 'images', startIndex = 0, aspect = '16 / 9', imgClassName = '',}) 
{
    const [idx, setIdx] = useState(
        names.length ? (startIndex % names.length + names.length) % names.length : 0
    );
    if (!names.length) {
        return null;
    } 

    const prev = () => setIdx((i) => (i - 1 + names.length) % names.length);
    const next = () => setIdx((i) => (i + 1) % names.length);

    return (
        <div>
            <Picture name={names[idx]} ext={ext} folder={folder} className={imgClassName} />
            <div className="mt-2 d-flex justify-content-center gap-2">
                <Button variant="outline-secondary" onClick={prev}>&laquo; Předchozí</Button>
                <Button variant="primary" onClick={next}>Další &raquo;</Button>
            </div>
            <div className="text-center text-muted small mt-1">
                {idx + 1} / {names.length}
            </div>
        </div>
    );
}

export default PictureSwitcher;
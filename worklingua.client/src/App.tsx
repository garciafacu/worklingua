import './App.css';
import { LocalizacionProvider } from './contexto/LocalizacionProvider';
import { SesionProvider } from './contexto/SesionProvider';
import { AppRouter } from './rutas/AppRouter';

function App() {
    return (
        // LocalizacionProvider va por fuera: carga los idiomas, la cultura y el
        // bundle de textos antes de que se renderice nada, para que la primera
        // pintura no muestre las claves crudas.
        <LocalizacionProvider>
            <SesionProvider>
                <AppRouter />
            </SesionProvider>
        </LocalizacionProvider>
    );
}

export default App;

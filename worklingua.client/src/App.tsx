import './App.css';
import { LocalizacionProvider } from './contexto/LocalizacionProvider';
import { SesionProvider } from './contexto/SesionProvider';
import { AppRouter } from './rutas/AppRouter';

function App() {
    return (
        <LocalizacionProvider>
            <SesionProvider>
                <AppRouter />
            </SesionProvider>
        </LocalizacionProvider>
    );
}

export default App;

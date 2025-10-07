const axios = require('axios');
const BASE_URL = 'http://localhost:5241';
async function testCreateZahtevOdmor() {
    console.log('🧪 TEST: Kreiranje zahteva za odmor');
    
    try {
        // 1. Login
        console.log('\n🔐 Login...');
        const loginResponse = await axios.post(`${BASE_URL}/api/auth/login`, {
            userName: 'admin',  // ✅ ISPRAVKA: koristi userName umesto email
            password: 'admin123'
        }, {
            timeout: 10000,
            httpsAgent: false
        });
        
        const token = loginResponse.data.Token;
        console.log('✅ Login uspešan!');
        
               // 2. Test podatke za zahtev sa UTC datumima
        const zahtevData = {
            zaposleniId: 4, // Milos Hovjecki
            datumOd: '2025-10-15T00:00:00.000Z',  // ✅ UTC format
            datumDo: '2025-10-17T00:00:00.000Z',   // ✅ UTC format
            razlog: 'Test zahtev za odmor',
            tipOdmora: 'Godisnji'
        };
        
        console.log('\n📝 Pokušavam kreiranje zahteva sa podacima:');
        console.log(JSON.stringify(zahtevData, null, 2));
        
        // 3. POST zahtev za odmor
        console.log('\n🎯 POST /api/zahtevzaodmor...');
        const response = await axios.post(`${BASE_URL}/api/zahtevzaodmor`, zahtevData, {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            timeout: 10000
        });
        
        console.log('✅ Zahtev uspešno kreiran!');
        console.log('Response:', response.data);
        
    } catch (error) {
        console.log('\n❌ GREŠKA!');
        
        if (error.response) {
            console.log('Status:', error.response.status);
            console.log('Zaglavlja:', error.response.headers);
            console.log('RESPONSE DATA (detaljne greške):');
            console.log('=====================================');
            console.log(JSON.stringify(error.response.data, null, 2));
            console.log('=====================================');
        } else if (error.request) {
            console.log('Nema odgovora od servera');
            console.log('Request:', error.request);
        } else {
            console.log('Greška u konfiguraciji:', error.message);
        }
    }
}
testCreateZahtevOdmor();